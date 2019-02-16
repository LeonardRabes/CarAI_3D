using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Simulation;

public enum carDisplayState
{
    ShowAll,
    RemoveInterfering,
    ShowBest
}

public class SimulationManager : MonoBehaviour
{
    public Engine simulationEngine;

    public Texture2D terrainImage;
    public GameObject sampleCar;
    public float carHeightOffset = 20;
    public float carMass = 20;
    public float carDrag = 10;

    public carDisplayState carDisplayState = carDisplayState.ShowAll;

    public float simulationLoopSpeed = 1;
    public Point simulationSpawn;
    public Point simulationTarget;

    public bool showDetails = false;
    public Material detailMaterialEyes;
    public Material detailMaterialHitbox;

    public CarGameObject[] carGameObjects;

    private Vector3 hideLocation;
    private byte[][][] terrainImagePixel;

    public class CarGameObject
    {
        public GameObject carObj;
        public GameObject carBodyObject;
        public GameObject[] carWheels;
        public bool isAlive;
        public bool isVisible;
        public int generation;
        public GameObject[] lines;
    }

    private void Start()
    {
        if (GloabalData.TrackData.TerrainImage != null)
        {
            terrainImage = GloabalData.TrackData.TerrainImage;
        }

        // Load Engine
        simulationSpawn = new Point((int)GloabalData.TrackData.SpawnPosition.x, (int)GloabalData.TrackData.SpawnPosition.y);
        simulationTarget = new Point((int)GloabalData.TrackData.TargetPosition.x, (int)GloabalData.TrackData.TargetPosition.y);

        terrainImagePixel = ParseTexture(terrainImage);
        simulationEngine = new Engine(terrainImagePixel, simulationSpawn, simulationTarget);
        simulationEngine.CarLength = 20;
        simulationEngine.CarWidth = 10;
        simulationEngine.SpawnRotation = GloabalData.TrackData.SpawnRotation;
        if (GloabalData.NetworkData.Structure == null)
        {
            simulationEngine.GenerateCars();
        }
        else
        {
            simulationEngine.LoadCarStructure(GloabalData.NetworkData.Structure);
        }



        // create Car game objects
        carGameObjects = new CarGameObject[simulationEngine.SpawnAmount];
        hideLocation = new Vector3(terrainImage.width / 2, 4096, terrainImage.height / 2);

        // instantiate each car
        for (int i = 0; i < carGameObjects.Length; i++)
        {
            CarGameObject car = new CarGameObject();
            car.carObj = GameObject.Instantiate<GameObject>(sampleCar);
            car.carObj.name = "car" + i;
            car.carObj.AddComponent<Rigidbody>();

            car.carObj.transform.parent = this.transform;
            car.carObj.transform.localScale = new Vector3(5, 5, 5);
            car.generation = -1;
            car.isAlive = false;
            car.isVisible = true;
            car.lines = new GameObject[simulationEngine.Cars[i].EyeAmount + 1];

            car.carBodyObject = car.carObj.transform.GetChild(0).gameObject;

            car.carWheels = FindChildrenByName("Wheel", car.carObj);

            Rigidbody body = car.carObj.GetComponent<Rigidbody>();
            body.collisionDetectionMode = CollisionDetectionMode.Discrete;
            body.detectCollisions = false;
            body.useGravity = false;
            body.mass = carMass;
            body.drag = carDrag;

            // Create LineRenderer Objcts to display details
            for (int l = 0; l < car.lines.Length; l++)
            {
                GameObject obj = new GameObject();
                obj.transform.parent = car.carObj.transform;
                LineRenderer renderer = obj.AddComponent<LineRenderer>();

                if (l < simulationEngine.Cars[i].EyeAmount)
                {
                    obj.name = "Eye" + l + "_" + i;
                    renderer.positionCount = 2;
                    renderer.material = detailMaterialEyes;
                }
                else
                {
                    obj.name = "Hitbox_" + i;
                    renderer.positionCount = 5;
                    renderer.material = detailMaterialHitbox;
                }

                car.lines[l] = obj;
            }
            Car simCar = simulationEngine.Cars[i];
            carGameObjects[i] = car;
        }

        // disable collision between cars
        foreach (var target in carGameObjects)
        {
            foreach (var car in carGameObjects)
            {
                Physics.IgnoreCollision(target.carObj.GetComponent<Collider>(), car.carObj.GetComponent<Collider>(), true);
            }
        }

        // Start Simulation
        Invoke("StartSimulation", 5);
    }

    private void Update()
    {
        simulationEngine.LoopSpeed = simulationLoopSpeed;
        for (int i = 0; i < carGameObjects.Length; i++)
        {
            // manage draw states 
            if (carDisplayState == carDisplayState.ShowAll)
            {
                carGameObjects[i].isVisible = true;
            }
            else if (carDisplayState == carDisplayState.RemoveInterfering)
            {

            }
            else if (carDisplayState == carDisplayState.ShowBest)
            {
                if (i == GetBestCar())
                {
                    carGameObjects[i].isVisible = true;
                }
                else
                {
                    carGameObjects[i].isVisible = false;
                }
            }

            ManageCar(i); // Handles cars and their position

            HideCar(carGameObjects[i], !carGameObjects[i].isVisible);
        }
    }

    public int GetBestCar()
    {
        Car[] cars = simulationEngine.Cars;
        int bestIndex = -1;

        if (simulationEngine.LoopSpeed >= 0.5)
        {
            int best = GridUnit.GetByLocation(simulationEngine.SpawnLocation, simulationEngine.ParkourGrid, simulationEngine.GridUnitSize).Value;
            for (int i = 0; i < cars.Length; i++)
            {
                if (cars[i]?.Distance < best && cars[i].Alive)
                {
                    best = cars[i].Distance;
                    bestIndex = i;
                }
            }
        }

        return bestIndex;
    }

    private void StartSimulation()
    {
        simulationEngine.LoopThread.Start();
    }

    private void DrawLines(CarGameObject carObj, Car simCar, bool hide = false, Vector3? hideLocation = null)
    {
        for (int l = 0; l < carObj.lines.Length; l++)
        {
            LineRenderer renderer = carObj.lines[l].GetComponent<LineRenderer>();
            float offset = carHeightOffset + 15;
            if (!hide)
            {
                if (l < simCar.EyeAmount)
                {
                    Vector3 centre = new Vector3(simCar.Centre.X, offset, terrainImage.height - simCar.Centre.Y);
                    Eye eye = simCar.Eyes[l];
                    Vector3 eyeLoc = new Vector3(eye.Location[eye.Location.Length - 1].X, offset, terrainImage.height - eye.Location[eye.Location.Length - 1].Y);

                    renderer.SetPosition(0, centre);
                    renderer.SetPosition(1, eyeLoc);
                }
                else
                {
                    Vector3 rightFront = new Vector3(simCar.RightFront.X, offset, terrainImage.height - simCar.RightFront.Y);
                    Vector3 leftFront = new Vector3(simCar.LeftFront.X, offset, terrainImage.height - simCar.LeftFront.Y);
                    Vector3 rightBack = new Vector3(simCar.RightBack.X, offset, terrainImage.height - simCar.RightBack.Y);
                    Vector3 leftBack = new Vector3(simCar.LeftBack.X, offset, terrainImage.height - simCar.LeftBack.Y);

                    renderer.SetPosition(0, leftFront);
                    renderer.SetPosition(1, rightFront);
                    renderer.SetPosition(2, rightBack);
                    renderer.SetPosition(3, leftBack);
                    renderer.SetPosition(4, leftFront);
                }
            }
            else
            {
                for (int i = 0; i < renderer.positionCount; i++)
                {
                    renderer.SetPosition(i, hideLocation.Value);
                }
            }

        }
    }

    private bool CarsIntersect(CarGameObject target, CarGameObject[] array)
    {
        foreach (var item in array)
        {
            if (target.carObj.GetInstanceID() != item.carObj.GetInstanceID())
            {
                Collider targetColl = target.carObj.GetComponent<Collider>();
                Collider itemColl = item.carObj.GetComponent<Collider>();
                float dist = 0;
                Vector3 direction = new Vector3();
                bool result = Physics.ComputePenetration(targetColl, targetColl.transform.position, targetColl.transform.rotation,
                    itemColl, itemColl.transform.position, itemColl.transform.rotation, out direction, out dist);
                if (result)
                {
                    return result;
                }
            }
        }

        return false;
    }

    private void ManageCar(int i)
    {
        Car simCar = simulationEngine.Cars[i];
        Rigidbody body = carGameObjects[i].carObj.GetComponent<Rigidbody>();

        if (simCar == null)
        {
            return;
        }

        if (simCar.Alive) // true if simCar is alive
        {
            if (simulationEngine.Generation > carGameObjects[i].generation) // Reset cars to start position
            {
                body.detectCollisions = false;
                body.useGravity = false;
                carGameObjects[i].isAlive = true;
                body.velocity = Vector3.zero;
                body.drag = carDrag;

                // reset to start position
                carGameObjects[i].carObj.transform.position = new Vector3(simCar.Centre.X, carHeightOffset + 1, terrainImage.height - simCar.Centre.Y);
                carGameObjects[i].generation = simulationEngine.Generation;
            }
            else if (simCar.Alive && carGameObjects[i].isAlive) // follow simCar if carObj && simCar is alive
            {
                if (simulationLoopSpeed >= 0.25)
                {
                    Vector3 centre = new Vector3(simCar.Centre.X, carHeightOffset + 1, terrainImage.height - simCar.Centre.Y);
                    Vector3 posDiff = centre - carGameObjects[i].carObj.transform.position;

                    // change position by force
                    body.AddForce(posDiff, ForceMode.VelocityChange);
                    carGameObjects[i].carObj.transform.eulerAngles = new Vector3(0, simCar.Rotation - 90, 0);
                }
                else // if loop speed is too fast for force
                {
                    carGameObjects[i].carObj.transform.position = new Vector3(simCar.Centre.X, carHeightOffset + 1, terrainImage.height - simCar.Centre.Y);
                    carGameObjects[i].carObj.transform.eulerAngles = new Vector3(0, simCar.Rotation - 90, 0);
                }


            }

            DrawLines(carGameObjects[i], simCar, !showDetails, Vector3.zero);  // Draw details
        }
        else if (carGameObjects[i].isAlive) // true if simCar recently died, executed once after death
        {
            if (CarsIntersect(carGameObjects[i], carGameObjects) && UnityEngine.Random.value > 0.4) // Reduce lag caused by physics calc
            {
                carGameObjects[i].carObj.transform.position = hideLocation;
            }
            else // carObj enables physics
            {
                body.detectCollisions = true;
                body.useGravity = true;
                Vector3 centre = new Vector3(simCar.Centre.X, carHeightOffset + 1, terrainImage.height - simCar.Centre.Y);
                Vector3 posDiff = centre - carGameObjects[i].carObj.transform.position;
                body.drag = 0.2F;
                body.AddForce(posDiff, ForceMode.VelocityChange);
            }

            carGameObjects[i].isAlive = false;
            DrawLines(carGameObjects[i], simCar, true, Vector3.zero); // hide details on death
        }
    }

    private void HideCar(CarGameObject car, bool hide)
    {
        MeshRenderer[] renderer = car.carObj.GetComponentsInChildren<MeshRenderer>();

        foreach (var item in renderer)
        {
            item.enabled = !hide;
        }
    }

    private GameObject[] FindChildrenByName(string contains, GameObject parent)
    {
        List<GameObject> list = new List<GameObject>();
        int count = parent.transform.childCount;

        for (int i = 0; i < count; i++)
        {
            GameObject obj = parent.transform.GetChild(i).gameObject;
            if (obj.name.Contains(contains))
            {
                list.Add(obj);
            }
        }

        return list.ToArray();
    }

    private Vector2 AngleToVector(float angle)
    {
        return new Vector2((float)Math.Sin(angle), -(float)Math.Cos(angle));
    }

    private byte[][][] ParseTexture(Texture2D texture)
    {
        byte[][][] pixelArray = new byte[texture.height][][];

        int bottomYAxis = texture.width - 1;
        for (int yaxis = 0; yaxis < texture.height; yaxis++) // which row
        {
            byte[][] pixelRow = new byte[texture.width][];

            int bottomXAxis = texture.width - 1;
            for (int xaxis = 0; xaxis < texture.width; xaxis++)// which pixel
            {
                byte[] pixel = new byte[4];
                UnityEngine.Color pixelColor = texture.GetPixel(xaxis, bottomYAxis);
                pixel[0] = Convert.ToByte(pixelColor.a * 255);
                pixel[1] = Convert.ToByte(pixelColor.r * 255);
                pixel[2] = Convert.ToByte(pixelColor.g * 255);
                pixel[3] = Convert.ToByte(pixelColor.b * 255);

                pixelRow[xaxis] = pixel;
            }
            pixelArray[yaxis] = pixelRow;
            bottomYAxis--;
        }

        return pixelArray;
    } 
}

