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
            notFinishedAgents--;
            if(notFinishedAgents > 0)
            {
                return;
            }

            for (int i = 0; i < agentsPerSeries; i++)
            {
                if(Agents.Length <= i + CurrentSeries * agentsPerSeries)
                {
                    return;
                }
                Agents[i + CurrentSeries * agentsPerSeries].CloseProcess();
            }
            CurrentSeries++;
            if(SimulationManager.Status != SimulationStatus.GENETICS)
                SimulationManager.ChangeStatus(SimulationStatus.PYTHON);
        }
    }
}