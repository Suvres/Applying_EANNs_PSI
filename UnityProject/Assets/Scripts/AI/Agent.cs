using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PSI
{ 
    public class Agent
    {
        private int id;
        private int outputCount;

        public event EventHandler OnFinished;
        public event EventHandler<float[]> OnOutputRecived;

        public bool Running
        { 
            get; 
            set; 
        }

        public bool Active
        {
            get;
            set;
        }

        public Genotype Genotype
        {
            get;
            set;
        }

        private AsyncProcess process;

        public Agent(int id, int parameterCount, int outputCount)
        {
            this.id = id;
            this.Genotype = PSI.Genotype.GenerateRandom(parameterCount, -1.0f, 1.0f);
            this.outputCount = outputCount;
        }

        public Agent(int id, Genotype genotype, int outputCount)
        {
            this.id = id;
            this.Genotype = genotype;
            this.outputCount = outputCount;
        }

        public void Run()
        {
            CreateProcess();
        }

        public void Finish()
        {
            Running = false;
        }

        private void Process_StandartTextReceived(object sender, string e)
        {
            if (Running == false && Active == false)
            {
                Active = true;
                return;
            }

            float[] output = new float[outputCount];

            string[] outputsStr = e.Replace('.', ',').Split(' ');
            if (outputsStr.Length < 2)
                return;

            for (int i = 0; i < outputCount; i++)
            {
                float.TryParse(outputsStr[i], out output[i]);
            }

            OnOutputRecived?.Invoke(this, output);
        }

        private void Process_ErrorTextReceived(object sender, string e)
        {
            //ConsoleHandle.Log("Error: " + e);
        }

        public void CreateProcess()
        {
            process = new AsyncProcess();

            string fileName = id.ToString() + ".json";
            Genotype.GenerateWeightsFile(fileName);

            string MainFile = "Assets/Scripts/Python/main_2.py";
            string WeightFileName = fileName;
            string OutputCount = outputCount.ToString();
            string[] arguments = new string[] { MainFile, WeightFileName, OutputCount };

            process.ErrorTextReceived += Process_ErrorTextReceived;
            process.StandartTextReceived += Process_StandartTextReceived;

            //process.ExecuteAsync("C:/Users/filek/AppData/Local/Programs/Python/Python39/python.exe", arguments);
            process.ExecuteAsync("B:/Programy/Python3_9/python.exe", arguments);
        }

        public void SendInput(float[] inputs)
        {
            if (inputs == null) 
                return;

            string arguments = "1";
            for (int i = 0; i < 5; i++)
            {
                arguments += " " + inputs[i].ToString();
            }
            arguments = arguments.Replace(',', '.');
            process.WriteLine(arguments);
        }

        public void CloseProcess()
        {
            Active = false;
            process.Close();
        }

        public void Reset()
        {
            Genotype.Evaluation = 0;
            Genotype.Fitness = 0;
            CreateProcess();
        }

        public void GenerateWeightsFile()
        {
            Genotype.GenerateWeightsFile(id.ToString() + ".json");
        }
    }
}
