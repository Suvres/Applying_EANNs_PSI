using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using General;

namespace PSI
{
    public class Genotype
    {
        private static Random randomizer = new Random();

        public float Evaluation
        {
            get;
            set;
        }

        public float Fitness
        {
            get;
            set;
        }

        private float[] parameters;

        public int ParameterCount
        {
            get
            {
                if (parameters == null) return 0;
                return parameters.Length;
            }
        }

        public float this[int index]
        {
            get { return parameters[index]; }
            set { parameters[index] = value; }
        }

        public Genotype(float[] Parameters)
        {
            parameters = Parameters;
            Fitness = 0;
        }

        public void SetRandomParameters(float minValue, float maxValue)
        {
            float range = maxValue - minValue;
            for (int i = 0; i < parameters.Length; i++)
            {
                parameters[i] = (float)((randomizer.NextDouble() * range) + minValue);
            }
        }

        public float[] GetParameterCopy()
        {
            float[] copy = new float[ParameterCount];
            for (int i = 0; i < ParameterCount; i++)
                copy[i] = parameters[i];

            return copy;
        }

        public bool GenerateWeightsFile(string Name)
        {
            uint[] Layers = AgentsManager.Get().Topology;

            string json = "[";
            int c = 0;
            int parametIndex = 0;

            for(int LayerIndex = 0; LayerIndex < Layers.Length - 1; LayerIndex++) //Loop over all layers
            {
                json += (c != 0 ? "," : "") + "[";
                for (int i = 0; i < Layers[LayerIndex]+1; i++) //Loop over all nodes of current layer
                {
                    json += (i != 0 ? "," : "") + "[";
                    for (int j = 0; j < Layers[LayerIndex] - 1; j++) //Loop over all nodes of next layer
                    {
                        json += (j == 0 ? "" : ",") + parameters[parametIndex].ToString().Replace(",", ".");
                        parametIndex++;
                    }

                    json += "]";
                }

                c = 1;
                json += "]";
            }

            json += "]";
            StreamWriter writer = new StreamWriter(Name);
            writer.Write(json);
            writer.Close();

            return true;
        }

        public static Genotype GenerateRandom(uint parameterCount, float minValue, float maxValue)
        {
            //Check arguments
            if (parameterCount == 0) return new Genotype(new float[0]);

            Genotype randomGenotype = new Genotype(new float[parameterCount]);
            randomGenotype.SetRandomParameters(minValue, maxValue);

            return randomGenotype;
        }
    }
}