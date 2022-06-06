using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PSI
{

    public enum SimulationStatus
    {
        START,
        PYTHON,
        GENETICS,
        FINISH
    }

    public class SimulationManager : MonoBehaviour
    {
        private static SimulationManager instance;

        private AgentsManager agentManager;
        private TrackManager trackManager;
        private GeneticManager geneticManager;

        [SerializeField] private SimulationStatus simulationStatus;
        public static SimulationStatus Status
        {
            get => instance.simulationStatus;
        }

        public static bool Running
        {
            get;
            private set;
        }

        public void Awake()
        {
            instance = this;
            agentManager = GetComponent<AgentsManager>();
            trackManager = GetComponent<TrackManager>();
            geneticManager = GetComponent<GeneticManager>();
            Running = false;
        }

        public void Start()
        {
            ChangeStatus(SimulationStatus.START);
        }

        public static void ChangeStatus(SimulationStatus NewStatus)
        {
            instance.simulationStatus = NewStatus;
            switch (instance.simulationStatus)
            {
                case SimulationStatus.START:
                    instance.agentManager.Spawn();
                    instance.trackManager.Spawn(instance.agentManager.AgentsPerSeries);
                    ChangeStatus(SimulationStatus.PYTHON);
                    break;
                case SimulationStatus.PYTHON:
                    Running = true;
                    instance.trackManager.Setup(instance.agentManager.Agents, instance.agentManager.CurrentSeries);
                    instance.agentManager.Run();
                    instance.trackManager.Link();
                    break;
                case SimulationStatus.GENETICS:
                    instance.geneticManager.Run(instance.agentManager.Agents);
                    ChangeStatus(SimulationStatus.PYTHON);
                    break;
                case SimulationStatus.FINISH:
                    break;
            }
        }

    }
}
