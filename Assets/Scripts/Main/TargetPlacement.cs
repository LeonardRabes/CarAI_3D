using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetPlacement : MonoBehaviour
{
    public SimulationManager simulationManager;

    private SphereCollider sphereCollider;
    public void Start()
    {
        sphereCollider = this.GetComponent<SphereCollider>();
        Vector2 target = GloabalData.TrackData.TargetPosition;
        this.transform.position = new Vector3(target.x, transform.position.y, 1024 - target.y);
    }

    public void FixedUpdate()
    {
        foreach (var car in simulationManager.carGameObjects)
        {
            if (car.isAlive)
            {
                float dist;
                bool collide = checkCollision(car.carObj, this.gameObject, out dist);
                if (collide && dist > sphereCollider.radius )
                {
                    car.isAlive = false;
                    car.carObj.GetComponent<Rigidbody>().drag = simulationManager.carDrag - 4;
                }
            }

        }
    }

    private bool checkCollision(GameObject obj1, GameObject obj2, out float dist)
    {
        Collider coll1 = obj1.GetComponent<Collider>();
        Collider coll2 = obj2.GetComponent<Collider>();

        Vector3 dir;

        bool result = Physics.ComputePenetration(coll1, obj1.transform.position, obj1.transform.rotation,
            coll2, obj2.transform.position, obj2.transform.rotation, out dir, out dist);

        return result;
    }

    private SimulationManager.CarGameObject GetByCollider(Collider other, SimulationManager manager)
    {
        int instanceID = other.GetInstanceID();

        foreach (var car in manager.carGameObjects)
        {
            Collider coll = car.carObj.GetComponent<Collider>();
            if (instanceID == coll.GetInstanceID())
            {
                return car;
            }
        }

        return null;
    }
}
