using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PSI
{

    public class AgentsManager : MonoBehaviour
    {
        private static AgentsManager instance;
        public static AgentsManager Instance
        {
            get => instance;
        }


        [SerializeField] private int series;
        [SerializeField] private int agentsPerSeries;

        [SerializeField] private uint[] topology;
        [SerializeField] private int outputCount;

        [SerializeField] private int notFinishedAgents;

        public uint[] Topology
        {
            get => topology;
        }

        public int AgentCount
        {
            get => series * agentsPerSeries;
        }

        public int AgentsPerSeries
        {
            get => agentsPerSeries;
        }

        public Agent[] Agents
        {
            get;
            private set;
        }

        public int CurrentSeries;

        public void Awake()
        {
            instance = this;
            CurrentSeries = 0;
            notFinishedAgents = 0;
        }

        private bool running = false;

        public void Update()
        {
            if (SimulationManager.Status != SimulationStatus.PYTHON)
                return;

            int runningCount = 0;
            int activeCount = 0;
            for (int i = 0; i < agentsPerSeries; i++)
            {
                int index = i + CurrentSeries * agentsPerSeries;
                activeCount += (Agents[index].Active) ? 1 : 0;
                runningCount += (Agents[index].Running) ? 1 : 0;
            }

            if (running == false && activeCount == agentsPerSeries)
            {
                running = true;
                for (int i = 0; i < agentsPerSeries; i++)
                {
                    Agents[i + CurrentSeries * agentsPerSeries].Running = true;
                }
                return;
            }

            if (running == true && runningCount == 0)
            {
                running = false;
                AgentFinished();
            }

           
        }
        public void Spawn()
        {
            int weightCount = 0;
            for (int i = 0; i < topology.Length; i++)
            {
                weightCount += (int)((topology[i] + 1) * topology[i]); // + 1 for bias node
            }

            List<Agent> agentsList = new List<Agent>(AgentCount);
            for(int i = 0; i < AgentCount; i++)
            {
                agentsList.Add(new Agent(i, weightCount, outputCount));
            }

            Agents = agentsList.ToArray();
        }

        public void Run()
        {
            if(CurrentSeries >= series)
            {
                CurrentSeries = 0;
                SimulationManager.ChangeStatus(SimulationStatus.GENETICS);
                return;
            }

            for (int i = 0; i < agentsPerSeries; i++)
            {
                Agents[i + CurrentSeries * agentsPerSeries].Run();
            }
            notFinishedAgents = agentsPerSeries;
        }

        public void AgentFinished()
        {
            //Debug.Log("----------------- Close series " + CurrentSeries);
            for (int i = 0; i < agentsPerSeries; i++)
            {
                int index = i + CurrentSeries * agentsPerSeries;
                Agents[index].CloseProcess();
            }
            CurrentSeries++;
            if(SimulationManager.Status != SimulationStatus.GENETICS)
                SimulationManager.ChangeStatus(SimulationStatus.PYTHON);
        }
    }
}