using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PSI
{
    public class AgentsManager : MonoBehaviour
    {
        private static AgentsManager instance;
        public static AgentsManager Get() { return instance; }

        [SerializeField]
        private int agentCount = 1;
        public int AgentCount
        {
            get
            {
                return agentCount;
            }
        }

        public int outputNum = 2;
        public uint[] Topology;

        public int WeightCount
        {
            get;
            private set;
        }

        public int AliveAgents;
        private Agent[] agents;

        public void Awake()
        {
            instance = this;
        }

        public void Start()
        {
            WeightCount = 0;
            for (int i = 0; i < Topology.Length; i++)
                WeightCount += (int)((Topology[i] + 1) * Topology[i]); // + 1 for bias node

            agents = TrackManager.Instance.Spawn(AgentCount);
            AliveAgents = AgentCount;
            TrackManager.Instance.Restart();
        }

        public void DescreaseAlive()
        {
            AliveAgents--;
            if(AliveAgents != 0)
            {
                return;
            }

            GetComponent<GeneticManager>().Cross(ref agents);

            AliveAgents = AgentCount;
            TrackManager.Instance.Restart();
        }

    }
}