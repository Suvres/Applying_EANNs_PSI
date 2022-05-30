using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PSI
{
    public class TrackManager : MonoBehaviour
    {
        public static TrackManager Instance
        {
            get;
            private set;
        }

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
            Instance = this;

            CalculateCheckpointPercentages();
        }

        private void CalculateCheckpointPercentages()
        {
            checkpoints[0].AccumulatedDistance = 0; //First checkpoint is start
                                                    //Iterate over remaining checkpoints and set distance to previous and accumulated track distance.
            for (int i = 1; i < checkpoints.Length; i++)
            {
                checkpoints[i].DistanceToPrevious = Vector2.Distance(checkpoints[i].transform.position, checkpoints[i - 1].transform.position);
                checkpoints[i].AccumulatedDistance = checkpoints[i - 1].AccumulatedDistance + checkpoints[i].DistanceToPrevious;
            }

            //Set track length to accumulated distance of last checkpoint
            TrackLength = checkpoints[checkpoints.Length - 1].AccumulatedDistance;

            //Calculate reward value for each checkpoint
            for (int i = 1; i < checkpoints.Length; i++)
            {
                checkpoints[i].RewardValue = (checkpoints[i].AccumulatedDistance / TrackLength) - checkpoints[i - 1].AccumulatedReward;
                checkpoints[i].AccumulatedReward = checkpoints[i - 1].AccumulatedReward + checkpoints[i].RewardValue;
            }
        }

        public Agent[] Spawn(int Count)
        {
            Agent[] agents = new Agent[Count];
            cars = new List<Car>();
            for (int i = 0; i < Count; i++)
            {
                var car = Instantiate(carPrefab, StartPoint.position, StartPoint.rotation) as GameObject;
                var controller = car.GetComponent<CarController>();
                controller.Construct(i, (uint)AgentsManager.Get().WeightCount);
                controller.Alive = true;
                cars.Add(new Car(controller, 0));
                agents[i] = controller.Agent;
            }
            return agents;
        }

        public void Update()
        {
            foreach(var car in cars)
            {
                car.Controller.Reward = GetCompletePerc(car.Controller, ref car.CheckpointIndex);
            }
        }

        private float GetCompletePerc(CarController car, ref uint curCheckpointIndex)
        {
            //Already all checkpoints captured
            if (curCheckpointIndex >= checkpoints.Length)
                return 1;

            //Calculate distance to next checkpoint
            float checkPointDistance = Vector2.Distance(car.transform.position, checkpoints[curCheckpointIndex].transform.position);

            //Check if checkpoint can be captured
            if (checkPointDistance <= checkpoints[curCheckpointIndex].CaptureRadius)
            {
                curCheckpointIndex++;
                car.CheckpointCaptured(); //Inform car that it captured a checkpoint
                return GetCompletePerc(car, ref curCheckpointIndex); //Recursively check next checkpoint
            }
            else
            {
                //Return accumulated reward of last checkpoint + reward of distance to next checkpoint
                return checkpoints[curCheckpointIndex - 1].AccumulatedReward + checkpoints[curCheckpointIndex].GetRewardValue(checkPointDistance);
            }
        }

        public void Restart()
        {
            foreach (var car in cars)
            {
                car.Controller.Restart();
                car.Controller.gameObject.transform.position = StartPoint.position;
                car.Controller.gameObject.transform.rotation = StartPoint.rotation;
            }
        }
    }
}