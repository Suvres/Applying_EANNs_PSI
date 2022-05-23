using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PSI
{
    public class CarController : MonoBehaviour
    {
        private const float MAX_CHECKPOINT_DELAY = 7;

        private CarMovement movement;
        private CarSensor sensor;

        private float timeSinceLastCheckpoint = 0.0f;

        private int id;

        public bool Alive
        {
            get;
            set;
        }

        public Agent Agent
        {
            get;
            set;
        }

        public float Reward
        {
            get { return Agent.Genotype.Evaluation; }
            set { Agent.Genotype.Evaluation = value; }
        }

        public void Awake()
        {
            movement = GetComponent<CarMovement>();
            sensor = GetComponentInChildren<CarSensor>();
        }

        public void Construct(int Id, uint parameterCount)
        {
            id = Id;
            Agent = new Agent("Car_"+id, parameterCount);
            Agent.controller = this;
        }

        public void msg(string txt)
        {
            Debug.Log(txt);
        }

        public void Update()
        {
            timeSinceLastCheckpoint += Time.deltaTime;
        }

        public void FixedUpdate()
        {
            if (!Alive)
                return;

            if (timeSinceLastCheckpoint > MAX_CHECKPOINT_DELAY)
            {
                // Die();
                Alive = false;
                return;
            }

            movement.SetInputs(Agent.Run(sensor.Values));
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(collision.gameObject.CompareTag("Wall"))
            {
                Die();
            }
        }

        public void Die()
        {
            Alive = false;
        }

        public void CheckpointCaptured()
        {
            timeSinceLastCheckpoint = 0;
        }

        public void Restart()
        {
            Alive = true;
            timeSinceLastCheckpoint = 0;
            Agent.Reset();
        }
    }
}
