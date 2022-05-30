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
        public float reward;

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

        float timer = 0.0f;
        public void Update()
        {
            timeSinceLastCheckpoint += Time.deltaTime;
            timer += Time.deltaTime;
            reward = Reward;
        }

        bool waiting = false;
        public void FixedUpdate()
        {
            if (!Alive)
                return;

            if (timeSinceLastCheckpoint > MAX_CHECKPOINT_DELAY)
            {
                Die();
                return;
            }

            if (waiting) 
                return;

            if (timer < 1.0f)
                return;
            timer = 0.0f;

            waiting = true;
            Agent.SendInput(sensor.Values);
        }

        public void ReciveOutput(float[] outputs)
        {
            //Agent.CloseProcess();
            movement.SetInputs(outputs);
            waiting = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Wall"))
            {
                Die();
            }
        }

        public void Die()
        {
            Alive = false;
            Agent.CloseProcess();
            AgentsManager.Get().DescreaseAlive();
        }

        public void CheckpointCaptured()
        {
            timeSinceLastCheckpoint = 0;
        }

        public void Restart()
        {
            movement.SetInputs(new float[] { 0f, 0f });
            Agent.Reset();
            Alive = true;
            timeSinceLastCheckpoint = 0;
        }
    }
}
