using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PSI
{
    public class CarController : MonoBehaviour
    {
        private const float MAX_CHECKPOINT_DELAY = 10;

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

        public void Update()
        {
            timeSinceLastCheckpoint += Time.deltaTime;
            reward = Reward;
        }

        bool waiting = false;
        public void FixedUpdate()
        {
            if (!Agent.Running)
                return;

            if (timeSinceLastCheckpoint > MAX_CHECKPOINT_DELAY)
            {
                Die();
                return;
            }

            if (waiting) 
                return;

            waiting = true;
            Agent.SendInput(sensor.Values);
        }

        public void ReciveOutput(float[] outputs)
        {
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
            movement.SetInputs(new float[] { 0f, 0f });
            Agent.Finish();
            AgentsManager.Instance.AgentFinished();
        }

        public void CheckpointCaptured()
        {
            timeSinceLastCheckpoint = 0;
        }

        public void Restart()
        {
            movement.SetInputs(new float[] { 0f, 0f });
            timeSinceLastCheckpoint = 0;
        }

        public void Link()
        {
            Agent.OnOutputRecived += Agent_OnOutputRecived;
            Agent.OnFinished += Agent_OnFinished;
        }

        private void Agent_OnFinished(object sender, System.EventArgs e)
        {
            AgentsManager.Instance.AgentFinished();
        }

        private void Agent_OnOutputRecived(object sender, float[] e)
        {
            movement.SetInputs(e);
            waiting = false;
        }
    }
}
