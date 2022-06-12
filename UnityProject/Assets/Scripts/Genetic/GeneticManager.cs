using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;

namespace PSI
{
    public enum CrossStrategy
    {
        RouletteWheelSelection,
        Elitism
    }

    public enum Mutation
    {
        WITH_MUTATION,
        NO_MUTATION
    }

    public class GeneticManager : MonoBehaviour
    {
        private static GeneticManager instance;

        [SerializeField] private CrossStrategy strategy;
        [SerializeField] private Mutation mutation;
        [SerializeField] private float mutationChance = 0.1f;
        [SerializeField] private float expectedFitness;

        [Range(0, 16)]
        [SerializeField] private int inheritancePoints;

        [Header("Elitism")]
        [Range(0.0f, 0.5f)]
        [SerializeField] private float percentageOfEliteGroup;

        public int GenerationNumber;

        private readonly System.Random randomizer = new System.Random();

        private StringBuilder results = new StringBuilder();

        public void Awake()
        {
            GenerationNumber = 0;
            results.AppendLine("Generation number;Average evalution;Best evalution;Average fitness;Best fitness");
        }

        public void Cross(Genotype A, Genotype B, out Genotype OutA, out Genotype OutB)
        {
            int currentInheritanceIndex = 0;
            int[] inheritanceIndex = new int[inheritancePoints + 1];
            for(int i = 0; i < inheritancePoints; i++)
            {
                int previousIndex = (i == 0)? 0 : inheritanceIndex[i - 1] + 1;
                inheritanceIndex[i] = randomizer.Next(previousIndex, A.ParameterCount - inheritancePoints + i);
            }

            inheritanceIndex[inheritanceIndex.Length - 1] = A.ParameterCount;

            OutA = new Genotype(A);
            OutB = new Genotype(B);

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
                    OutA[i] = B[i];
                    OutB[i] = A[i];
                }
            }
        }

        public void Mutate(Genotype[] genotypes, int startIndex, int endIndex)
        {
            int chance = randomizer.Next(0, 100);
            if(mutationChance < (float)chance/100.0f)
            {
                return;
            }

            int index = randomizer.Next(startIndex, endIndex);
            int parameterIndex = randomizer.Next(0, genotypes[index].ParameterCount);
            genotypes[index][parameterIndex] = (float)((randomizer.NextDouble() * 1.0) - 1.0f);
        }

        public Genotype[] Strategy_1(Genotype[] genotypes)
        {
            Array.Sort(genotypes, 0, genotypes.Length);

            float fitnessSum = 0.0f;
            foreach(Genotype genotype in genotypes)
            {
                fitnessSum += genotype.Fitness;
            }

            float[] probability = new float[genotypes.Length];
            for(int i = genotypes.Length - 1; i >= 0; i--)
            {
                probability[i] = genotypes[i].Fitness / fitnessSum;
            }

            Genotype[] newGenotypes = new Genotype[genotypes.Length];
            for (int i = 0; i < genotypes.Length; i++)
            {
                double random = randomizer.NextDouble() * 1.0;

                float sum = 0.0f;

                for(int j = 0; j <genotypes.Length; j++)
                {
                    sum += probability[j];
                    if (sum > random)
                    {
                        Genotype outA, outB;
                        Cross(genotypes[i], genotypes[j], out outA, out outB);

                        newGenotypes[i] = outA;
                        continue;
                    }
                }
            }

            if (mutation == Mutation.WITH_MUTATION)
            {
                Mutate(genotypes, 0, genotypes.Length);
            }

            return newGenotypes;
        }

        public Genotype[] Strategy_2(Genotype[] genotypes)
        {
            Array.Sort(genotypes, 0, genotypes.Length);
            int elitGroupSize = (int)(percentageOfEliteGroup * genotypes.Length);

            Genotype[] newGenotypes = new Genotype[genotypes.Length];
            List<int> openIndices = new List<int>();
            for (int i = 0; i < elitGroupSize; i++)
            {
                newGenotypes[i] = new Genotype(genotypes[i]);
                openIndices.Add(i);
            }

            int lastNewIndex = elitGroupSize;
            for (int i = 0; i < elitGroupSize; i += 2)
            {
                int index = randomizer.Next(0, openIndices.Count);
                int Aindex = openIndices[index];
                openIndices.RemoveAt(index);
                index = randomizer.Next(0, openIndices.Count);
                int Bindex = openIndices[index];
                var A = genotypes[Aindex];
                var B = genotypes[Bindex];
                Genotype OutA, OutB;
                Cross(A, B, out OutA, out OutB);
                newGenotypes[lastNewIndex++] = OutA;
                newGenotypes[lastNewIndex++] = OutB;
            }

            int parameterCount = genotypes[0].ParameterCount;
            for (int i = lastNewIndex; i < genotypes.Length; i++)
            {
                newGenotypes[i] = Genotype.GenerateRandom(parameterCount, -1.0f, 1.0f);
            }

            if (mutation == Mutation.WITH_MUTATION)
            {
                Mutate(genotypes, 0, genotypes.Length);
            }

            return newGenotypes;
        }

        public void Run(Agent[] agents)
        {
            float averageEvaluation = 0.0f;
            float bestEvaluation = 0.0f;
            foreach (Agent agent in agents)
            {
                averageEvaluation += agent.Genotype.Evaluation;
                if (bestEvaluation < agent.Genotype.Evaluation)
                    bestEvaluation = agent.Genotype.Evaluation;
            }

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

            Debug.Log("[Evaluation] Average: " + averageEvaluation / agents.Length+ " Best: " + bestEvaluation + " | [Fitness] Average: " + averageFitness + " Best: " + bestFitness);
            results.AppendLine(GenerationNumber + ";" + 
                (averageEvaluation / agents.Length) + ";" + 
                bestEvaluation + ";" + 
                averageFitness + ";" + 
                bestFitness);

            Genotype[] genotypes = new Genotype[agents.Length];
            for(int i = 0; i < agents.Length; i++)
            {
                genotypes[i] = agents[i].Genotype;
            }

            Genotype[] newGenotypes = null;

            switch (strategy)
            {
                case CrossStrategy.RouletteWheelSelection:
                    newGenotypes = Strategy_1(genotypes);
                    break;
                case CrossStrategy.Elitism:
                    newGenotypes = Strategy_2(genotypes);
                    break;
            }

            for (int i = 0; i < agents.Length; i++)
            {
                agents[i].Genotype = newGenotypes[i];
            }


            foreach (Agent agent in agents)
            {
                agent.GenerateWeightsFile();
            }
            GenerationNumber++;
        }

        public void OnApplicationQuit()
        {
            StreamWriter writer = new StreamWriter("results.csv");
            writer.Write(results.ToString());
            writer.Close();
        }
    }
}
