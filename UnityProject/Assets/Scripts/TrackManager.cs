using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PSI
{
    public class TrackManager : MonoBehaviour
    {
        private static TrackManager instance;

        public float TrackLength
        {
            get;
            private set;
        }

        [SerializeField]
        private Checkpoint[] checkpoints;

        [SerializeField]
        private Transform StartPoint;

        [SerializeField]
        private GameObject carPrefab;

        [SerializeField]
        private CameraController camera;



        private class Car
        {
            public Car(CarController controller = null, uint checkpointIndex = 1)
            {
                this.Controller = controller;
                this.CheckpointIndex = checkpointIndex;
            }
            public CarController Controller;
            public uint CheckpointIndex;
        }
        private List<Car> cars;

        public void Awake()
        {
            instance = this;

            CalculateCheckpointPercentages();
        }

        private void CalculateCheckpointPercentages()
        {
            for (int i = 0; i < checkpoints.Length - 1; i++)
            {
                checkpoints[i].DistanceToNext = Vector3.Distance(checkpoints[i].transform.position, checkpoints[i + 1].transform.position);
            }
        }

        public void Spawn(int agentsPerSeries)
        {
            cars = new List<Car>();
            for (int i = 0; i < agentsPerSeries; i++)
            {
                var car = Instantiate(carPrefab, StartPoint.position, StartPoint.rotation);
                var controller = car.GetComponent<CarController>();
                controller.id = i;
                cars.Add(new Car(controller, 0));
            }
        }

        public void Setup(Agent[] agents, int seriesNumber)
        {
            if (seriesNumber * cars.Count >= agents.Length)
                return;

            foreach (var car in cars)
            {
                car.Controller.Restart();
                car.Controller.gameObject.transform.position = StartPoint.position;
                car.Controller.gameObject.transform.rotation = StartPoint.rotation;
            }

            int i = 0;
            foreach (var car in cars)
            {
                car.Controller.Agent = agents[seriesNumber * cars.Count + i];
                i++;
            }
        }

        public void Link()
        {
            foreach (var car in cars)
            {
                car.Controller.Link();
            }
        }

        public void Update()
        {
            if (!SimulationManager.Running)
                return;

            float bestCarReward = -1000.0f;
            CarController bestCar = null;

            foreach(var car in cars)
            {
                if (car.Controller.Agent.Running)
                {
                    car.Controller.Reward = GetCompletePerc(car.Controller, ref car.CheckpointIndex);
                    if(bestCarReward < car.Controller.Reward)
                    {
                        bestCar = car.Controller;
                    }
                }
            }

            if (bestCar)
            {
                camera.SetPosition(bestCar.transform.position);
            }
        }

        private float GetCompletePerc(CarController car, ref uint curCheckpointIndex)
        {
            if(curCheckpointIndex >= checkpoints.Length)
            {
                return checkpoints.Length;
            }

            if(curCheckpointIndex + 1 >= checkpoints.Length)
            {
                return checkpoints.Length * 1.5f;
            }
            float distanceToNext = Vector3.Distance(car.transform.position, checkpoints[curCheckpointIndex + 1].transform.position);
            if(distanceToNext < 2.0f)
            {
                curCheckpointIndex++;
                car.CheckpointCaptured();
                return curCheckpointIndex / checkpoints.Length;
            }


            float currentPercentage = ( checkpoints[curCheckpointIndex].DistanceToNext - distanceToNext )/ checkpoints[curCheckpointIndex].DistanceToNext;
            return (curCheckpointIndex + currentPercentage) / checkpoints.Length;
        }
    }
}