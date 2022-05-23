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
            
            string arguments = "/home/suvres/Dokumenty/others/Applying_EANNs_PSI/UnityProject/Assets/Scripts/Python/main.py";
            arguments += " /home/suvres/Dokumenty/others/Applying_EANNs_PSI/UnityProject/" + fileName;
            foreach(float input in inputs)
            {
                arguments += " " + input.ToString();
            }
            arguments += " " + AgentsManager.Get().outputNum.ToString();

            controller.msg(arguments);

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/usr/bin/python3.8",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = false
                }
            };

            proc.Start();

            string line = proc.StandardOutput.ReadToEnd();
            string error = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            controller.msg("output: "+line);
            controller.msg("error: "+error);

            float[] outputs = new float[AgentsManager.Get().outputNum];

            string[] outputsStr = line.Replace('.', ',').Split(' ');
            for (int i = 0; i < outputsStr.Length; i++)
            { 
                controller.msg(outputsStr[i]);
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
