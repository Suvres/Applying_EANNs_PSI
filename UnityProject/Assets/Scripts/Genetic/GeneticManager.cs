using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PSI
{
    public class GeneticManager : MonoBehaviour
    {
        private System.Random randomizer = new System.Random();

        public int BestCount = 2;
        public int InheritancePoints = 1;

        public void Cross(ref Agent[] agents)
        {
            Array.Sort(agents, 0, agents.Length);

            for(int i = 0; i < agents.Length; i++)
            {
                var A = agents[randomizer.Next(0, BestCount)].Genotype;
                var B = agents[randomizer.Next(0, BestCount)].Genotype;
                CrossAlgorithm_1(ref A, ref B);
            }
        }

        public void CrossAlgorithm_1(ref Genotype A, ref Genotype B)
        {
            int currentInheritanceIndex = 0;
            int[] inheritanceIndex = new int[InheritancePoints + 1];
            for(int i = 0; i < InheritancePoints; i++)
            {
                int previousIndex = (i == 0)? 0 : inheritanceIndex[i - 1] + 1;
                inheritanceIndex[i] = randomizer.Next(previousIndex, A.ParameterCount - InheritancePoints + i);
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

    }
}
