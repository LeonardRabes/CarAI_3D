using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;

namespace Simulation
{
    public class Engine : IDisposable
    {
        public Thread LoopThread;
        public long LoopIterations = 0;
        public bool LoopActive = true;
        public float LoopSpeed = 1.0F; // 1.0 = 30 iterations per second
        public long LoopsPerSecond;

        public byte[][][] ParkourPixel;
        public GridUnit[][] ParkourGrid;
        public int GridUnitSize = 4;
        public int GridMaxValue = 0;

        public Car[] Cars;
        public Car[] CarRanking;
        public int SpawnAmount = 100;
        public Point SpawnLocation;
        public Point TargetLocation;
        public float SpawnRotation = 0.0F;
        public int CarLength = 60;
        public int CarWidth = 30;

        private Thread loopCounterThread;

        public int Generation = 0;
        private int generationTime = 0;
        private int genCarsAlive;

        public Engine()
        {

        }

        public Engine(byte[][][] parkourPixel, Point spawn, Point target)
        {
            //Parse image, divide into grid, generate path
            ParkourPixel = parkourPixel;
            ParkourGrid = GridUnit.CreateGrid(ParkourPixel, GridUnitSize);
            SpawnLocation = spawn;
            TargetLocation = target;
            GridUnit.FindPath(TargetLocation, ParkourGrid, GridUnitSize);
            GridMaxValue = GridUnit.GetByLocation(TargetLocation, ParkourGrid, GridUnitSize).Value;

            LoopThread = new Thread(new ThreadStart(EngineLoop));
            loopCounterThread = new Thread(IterationCounter);
            loopCounterThread.Start();
            //GenerateCars();
        }

        public void LoadCarStructure(string structure)
        {
            GenerateCars(structure);
        }

        public void LoadCarStructure(Stream structure)
        {
            GenerateCars(structure);
        }

        public void StopEngine()
        {
            LoopThread.Abort();
            loopCounterThread.Abort();
        }

        private void EngineLoop()
        {
            while (LoopActive)
            {
                foreach (Car car in Cars)
                {
                    if(car != null)
                    {
                        car.Drive(generationTime);
                        checkCollision(car);
                    }
                }

                if (genCarsAlive <= 0 || generationTime > 1500)
                {
                    CarRanking = RankCars(Cars);
                    GenerateCars(CarRanking);
                    Generation++;
                    generationTime = 0;
                }

                generationTime++;
                LoopIterations++;
                if (LoopSpeed > 0)
                    Thread.Sleep(Convert.ToInt32(30 * LoopSpeed));
            }

            void checkCollision(Car car)
            {
                byte[] black = new byte[] { 255, 0, 0, 0 };
                byte[] red = new byte[] { 255, 255, 0, 0 };

                byte[] rfront = ParkourPixel[Math.Min(Math.Max(0, Convert.ToInt32(car.RightFront.Y)), ParkourPixel.Length - 1)][Math.Min(Math.Max(0, Convert.ToInt32(car.RightFront.X)), ParkourPixel[0].Length - 1)];
                byte[] lfront = ParkourPixel[Math.Min(Math.Max(0, Convert.ToInt32(car.LeftFront.Y)), ParkourPixel.Length - 1)][Math.Min(Math.Max(0, Convert.ToInt32(car.LeftFront.X)), ParkourPixel[0].Length - 1)];
                byte[] rback =  ParkourPixel[Math.Min(Math.Max(0, Convert.ToInt32(car.RightBack.Y)), ParkourPixel.Length - 1)][Math.Min(Math.Max(0, Convert.ToInt32(car.RightBack.X)), ParkourPixel[0].Length - 1)];
                byte[] lback =  ParkourPixel[Math.Min(Math.Max(0, Convert.ToInt32(car.LeftBack.Y)), ParkourPixel.Length - 1)][Math.Min(Math.Max(0, Convert.ToInt32(car.LeftBack.X)), ParkourPixel[0].Length - 1)];

                if (compareColor(rfront, black) || compareColor(lfront, black) || compareColor(rback, black) || compareColor(lback, black))
                {
                    if (car.Alive)
                    {
                        car.Alive = false;
                        car.Lifetime = generationTime;
                        genCarsAlive--;
                    }
                }

                if (compareColor(rfront, red) || compareColor(lfront, red) || compareColor(rback, red) || compareColor(lback, red))
                {
                    if (car.Alive)
                    {
                        car.Alive = false;
                        car.Lifetime = generationTime;
                        genCarsAlive--;
                    }
                    //MessageBox.Show("Test");
                    //GenerateCars();
                }

                bool compareColor(byte[] c1, byte[] c2)
                {
                    return c1[0] == c2[0] && c1[1] == c2[1] && c1[2] == c2[2] && c1[3] == c2[3];
                }
            }
        }

        private void IterationCounter()
        {
            long iterations;
            while (LoopActive)
            {
                iterations = LoopIterations;
                Thread.Sleep(250);
                LoopsPerSecond = (LoopIterations - iterations) * 4;
            }
        }

        public void GenerateCars()
        {
            Cars = new Car[SpawnAmount];
            genCarsAlive = SpawnAmount;
            for (int i = 0; i < SpawnAmount; i++)
            {
                Cars[i] = new Car(SpawnLocation, ParkourPixel, this);
                Cars[i].Rotation = SpawnRotation;
                Cars[i].Width = CarWidth;
                Cars[i].Length = CarLength;
            }
        }

        private void GenerateCars(Car[] sortedCars)
        {
            List<Car> survivingCars = new List<Car>();
            float rankPercentage = 100 / SpawnAmount;
            bool notEnoughCarsAlive = true;

            while (notEnoughCarsAlive)
            {
                for (int i = sortedCars.Length - 1; i >= 0; i--)
                {
                    float survivalPercentage = rankPercentage * (i + 1);
                    float rndNum = RandomNumber.Between(0, 100);
                    if (rndNum <= survivalPercentage && survivingCars.Count < SpawnAmount / 2)
                    {
                        survivingCars.Add(sortedCars[i]);
                    }
                }

                if (survivingCars.Count >= SpawnAmount / 2)
                    notEnoughCarsAlive = false;
            }

            Cars = new Car[SpawnAmount];
            int carCount = 0;
            genCarsAlive = SpawnAmount;

            for (int i = 0; i < SpawnAmount / 2; i++)
            {
                Cars[carCount] = new Car(SpawnLocation, survivingCars[i], ParkourPixel, this);
                Cars[carCount + 1] = new Car(SpawnLocation, survivingCars[i], ParkourPixel, this);
                survivingCars[i] = null;

                Cars[carCount].Rotation = SpawnRotation;
                Cars[carCount].Centre = SpawnLocation;
                Cars[carCount].Alive = true;

                Cars[carCount + 1].Rotation = SpawnRotation;
                Cars[carCount + 1].Centre = SpawnLocation;
                Cars[carCount + 1].Alive = true;

                carCount += 2;
            }

            survivingCars = null;
        }

        private void GenerateCars(string structure)
        {
            Cars = new Car[SpawnAmount];
            genCarsAlive = SpawnAmount;
            for (int i = 0; i < SpawnAmount; i++)
            {
                Cars[i] = new Car(SpawnLocation, structure, ParkourPixel, this);
                Cars[i].Rotation = SpawnRotation;
                Cars[i].Width = CarWidth;
                Cars[i].Length = CarLength;
            }
        }

        private void GenerateCars(Stream structure)
        {
            Cars = new Car[SpawnAmount];
            genCarsAlive = SpawnAmount;
            for (int i = 0; i < SpawnAmount; i++)
            {
                Cars[i] = new Car(SpawnLocation, structure, ParkourPixel, this);
                Cars[i].Rotation = SpawnRotation;
                Cars[i].Width = CarWidth;
                Cars[i].Length = CarLength;
            }
        }

        private Car[] RankCars(Car[] prevCars)
        {
            List<Car> cars = new List<Car>(prevCars);
            foreach (Car car in cars)
            {
                car.Distance = GridUnit.GetByLocation(new Point(Convert.ToInt32(car.Centre.X), Convert.ToInt32(car.Centre.Y)), ParkourGrid, GridUnitSize).Value;
            }

            Car[] sortedCars = cars.OrderBy(car => car.Distance).ThenBy(car => car.Lifetime).Reverse().ToArray();

            return sortedCars;
        }

        public void Dispose()
        {
            ParkourPixel = null;
            ParkourGrid = null;
            Cars = null;
            CarRanking = null;
        }
    }
}
