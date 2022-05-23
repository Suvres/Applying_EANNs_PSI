using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace PSI
{
    public class Agent
    {
        private string Name;

        public Genotype Genotype
        {
            get;
            private set;
        }

        public CarController controller;

        public Agent(string Name, uint parameterCount)
        {
            this.Name = Name;
            Genotype = PSI.Genotype.GenerateRandom(parameterCount, -1.0f, 1.0f);
        }

        public float[] Run(float[] inputs)
        {
            string fileName = Name + ".json";
            Genotype.GenerateWeightsFile(fileName);

            string arguments = "B:\\Projekty\\Applying_EANNs_PSI\\UnityProject\\Assets\\Scripts\\Python\\main.py";
            arguments += " " + fileName;
            foreach(float input in inputs)
            {
                arguments += " B:\\Projekty\\Applying_EANNs_PSI\\UnityProject\\" + input.ToString();
            }
            arguments += " " + AgentsManager.Get().outputNum.ToString();

            controller.msg(arguments);

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "C:\\Users\\Alek\\AppData\\Local\\Programs\\Python\\Python39\\python.exe",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = false
                }
            };

            proc.Start();

            string line = proc.StandardOutput.ReadToEnd();

            proc.WaitForExit();

            controller.msg(line);

            float[] outputs = new float[AgentsManager.Get().outputNum];

            string[] outputsStr = line.Split(' ');
            for (int i = 0; i < outputsStr.Length; i++)
            {
                outputs[i] = float.Parse(outputsStr[i]);
            }

            return outputs;
        }

        public void Reset()
        {
            Genotype.Evaluation = 0;
            Genotype.Fitness = 0;
        }
    }
}
