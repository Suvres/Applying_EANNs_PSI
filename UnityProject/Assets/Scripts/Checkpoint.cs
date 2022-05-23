using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PSI
{
    public class Checkpoint : MonoBehaviour
    {
        public float CaptureRadius = 3;

        public float RewardValue
        {
            get;
            set;
        }

        public float DistanceToPrevious
        {
            get;
            set;
        }

        public float AccumulatedDistance
        {
            get;
            set;
        }

        public float AccumulatedReward
        {
            get;
            set;
        }

        public float GetRewardValue(float currentDistance)
        {
            //Calculate how close the distance is to capturing this checkpoint, relative to the distance from the previous checkpoint
            float completePerc = (DistanceToPrevious - currentDistance) / DistanceToPrevious;

            //Reward according to capture percentage
            if (completePerc < 0)
                return 0;
            else 
                return completePerc * RewardValue;
        }
    }
}
