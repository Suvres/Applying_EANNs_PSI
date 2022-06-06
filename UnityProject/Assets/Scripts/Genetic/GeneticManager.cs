using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PSI
{
    public class GeneticManager : MonoBehaviour
    {
        private static GeneticManager instance;

        [SerializeField] private float expectedFitness;
        [SerializeField] private float percentageOfAgentsToCross;
        [SerializeField] private int inheritancePoints;

        public int GenerationNumber;

        private readonly System.Random randomizer = new System.Random();

        public void Awake()
        {
            GenerationNumber = 0;
        }

        public void CrossAlgorithm_1(ref Genotype A, ref Genotype B)
        {
            int currentInheritanceIndex = 0;
            int[] inheritanceIndex = new int[inheritancePoints + 1];
            for(int i = 0; i < inheritancePoints; i++)
            {
                int previousIndex = (i == 0)? 0 : inheritanceIndex[i - 1] + 1;
                inheritanceIndex[i] = randomizer.Next(previousIndex, A.ParameterCount - inheritancePoints + i);
            }

            inheritanceIndex[inheritanceIndex.Length - 1] = A.ParameterCount;

            bool swap = false;
            for(int i = 0; i < A.ParameterCount; i++)
            {
                if(inheritanceIndex[currentInheritanceIndex] == i)
                {
                    swap = !swap;
                    currentInheritanceIndex++;
                }

                if(swap)
                {
                    float temp = A[i];
                    A[i] = B[i];
                    B[i] = temp;
                }
            }
        }

        public void Mutate_1(ref Genotype genotype)
        {

        }

        public void Run(Agent[] agents)
        {
            float averageEvaluation = 0.0f;
            foreach(Agent agent in agents)
            {
                averageEvaluation += agent.Genotype.Evaluation;
            }

            averageEvaluation /= agents.Length;

            float averageFitness = 0.0f;
            float bestFitness = 0.0f;
            foreach (Agent agent in agents)
            {
                agent.Genotype.Fitness = agent.Genotype.Evaluation / averageEvaluation;
                averageFitness += agent.Genotype.Fitness;
                if(bestFitness < agent.Genotype.Fitness)
                    bestFitness = agent.Genotype.Fitness;
            }

            averageFitness /= agents.Length;
            Debug.Log("Average fitness: " + averageFitness);
            Debug.Log("Best fitness: " + bestFitness);

            Array.Sort(agents, 0, agents.Length);
            int bestGroupSize = (int)(percentageOfAgentsToCross * agents.Length);

            for (int i = 0; i < bestGroupSize; i++)
            {
                var A = agents[randomizer.Next(0, bestGroupSize)].Genotype;
                var B = agents[randomizer.Next(0, bestGroupSize)].Genotype;
                CrossAlgorithm_1(ref A, ref B);
            }
            
            int parameterCount = agents[0].Genotype.ParameterCount;
            for (int i = bestGroupSize; i < agents.Length; i++)
            {
                agents[i].Genotype = Genotype.GenerateRandom(parameterCount, -1.0f, 1.0f);
            }
            
            foreach(Agent agent in agents)
            {
                agent.GenerateWeightsFile();
            }
            GenerationNumber++;
        }
    }
}
