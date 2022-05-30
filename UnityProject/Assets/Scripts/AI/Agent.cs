using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
/*
public static Task WaitForExitAsync(this Process process, CancellationToken cancellationToken = default(CancellationToken))
{
    if (process.HasExited) return Task.CompletedTask;

    var tcs = new TaskCompletionSource<object>();
    process.EnableRaisingEvents = true;
    process.Exited += (sender, args) => tcs.TrySetResult(null);
    if (cancellationToken != default(CancellationToken))
        cancellationToken.Register(() => tcs.SetCanceled());

    return process.HasExited ? Task.CompletedTask : tcs.Task;
}
*/
namespace PSI
{ 
    public class Agent : IComparer, IComparable
    {
        private string Name;

        public Genotype Genotype
        {
            get;
            private set;
        }

        public CarController controller;
        public AsyncProcess process;

        public Agent(string Name, uint parameterCount)
        {
            this.Name = Name;
            Genotype = PSI.Genotype.GenerateRandom(parameterCount, -1.0f, 1.0f);
            process = new AsyncProcess("b:/programy/python3_9/python.exe", false);

            //CreateProcess();
        }

        ~Agent()
        {
            CloseProcess();
        }

        private void Process_StandartTextReceived(object sender, string e)
        {
            //controller.msg(e);

            float[] outputs = new float[AgentsManager.Get().outputNum];

            string[] outputsStr = e.Replace('.', ',').Split(' ');
            for (int i = 0; i < outputsStr.Length; i++)
            {
                float.TryParse(outputsStr[i], out outputs[i]);
            }

            controller.ReciveOutput(outputs);
        }

        private void Process_ErrorTextReceived(object sender, string e)
        {
            controller.msg(e);
            //controller.Die();
        }

        public void CreateProcess()
        {
            string fileName = Name + ".json";
            Genotype.GenerateWeightsFile(fileName);

            string MainFile = "Assets/Scripts/Python/main_2.py";
            string WeightFileName = fileName;
            string OutputCount = AgentsManager.Get().outputNum.ToString();
            string[] arguments = new string[] { MainFile, WeightFileName, OutputCount };

            process.ErrorTextReceived += Process_ErrorTextReceived;
            process.StandartTextReceived += Process_StandartTextReceived;

            process.ExecuteAsync(arguments);
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
            //controller.msg(arguments);
            process.WriteLine(arguments);
        }

        public void CloseProcess()
        {
            process.Close();
        }

        public void Reset()
        {
            Genotype.Evaluation = 0;
            Genotype.Fitness = 0;
            CreateProcess();
        }

        public int Compare(object x, object y)
        {
            return  (((Agent)x).Genotype.Evaluation < ((Agent)y).Genotype.Evaluation)?-1 : 
                (((Agent)x).Genotype.Evaluation > ((Agent)y).Genotype.Evaluation) ? 1 : 0;
        }

        public void GenerateWeightsFile()
        {
            Genotype.GenerateWeightsFile(Name);
        }

        public int CompareTo(object obj)
        {
            return (((Agent)this).Genotype.Evaluation < ((Agent)obj).Genotype.Evaluation) ? -1 :
                (((Agent)this).Genotype.Evaluation > ((Agent)obj).Genotype.Evaluation) ? 1 : 0;
        }
    }
}
