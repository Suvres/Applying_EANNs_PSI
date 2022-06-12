using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PSI
{
    public class CarController : MonoBehaviour
    {
        private const float MAX_CHECKPOINT_DELAY = 10;

        private CarMovement movement;
        private CarSensor sensor;

        public float timeSinceLastCheckpoint = MAX_CHECKPOINT_DELAY;

        public int id;

        private Text text;

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
            text = GetComponentInChildren<Text>();
        }

        public void Update()
        {
            timeSinceLastCheckpoint -= Time.deltaTime;
            text.text = Reward.ToString();
        }

        private bool waiting = false;

        public void FixedUpdate()
        {
            if (Agent.Running == false)
            {
                return;
            }

            if (timeSinceLastCheckpoint <= 0.0f)
            {
                Die("timeout");
                return;
            }

            if (waiting)
            {
                return;
            }

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
                Die("collision");
            }
        }

        public void Die(string Context)
        {
            //Debug.Log("Car " + id + " " + Context);
            movement.SetInputs(new float[] { 0f, 0f });
            Agent.Finish();
            //AgentsManager.Instance.AgentFinished();
        }

        public void CheckpointCaptured()
        {
            timeSinceLastCheckpoint = MAX_CHECKPOINT_DELAY;
        }

        public void Restart()
        {
            movement.SetInputs(new float[] { 0f, 0f });
            timeSinceLastCheckpoint = MAX_CHECKPOINT_DELAY;
            waiting = false;
            movement.Reset();
        }

        public void Link()
        {
            Agent.OnOutputRecived += Agent_OnOutputRecived;
            Agent.OnFinished += Agent_OnFinished;
        }

        private void Agent_OnFinished(object sender, System.EventArgs e)
        {
            //AgentsManager.Instance.AgentFinished();
        }

        private void Agent_OnOutputRecived(object sender, float[] e)
        {
            movement.SetInputs(e);
            waiting = false;
        }
    }
}
