using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace PSI
{
    public class AsyncProcess
    {
        private string appName;
        private Process process;
        private readonly object theLock = new object();
        private SynchronizationContext context;
        public string pendingWriteData;


        public event EventHandler<string> ErrorTextReceived;
        public event EventHandler ProcessExited;
        public event EventHandler<string> StandartTextReceived;

        private Task StarnardReadTask;
        private Task ErrorReadTask;
        private Task WriteTask;

        private StreamWriter writer;

        public int ExitCode
        {
            get { return this.process.ExitCode; }
        }

        public bool Running
        {
            get; private set;
        }

        public void ExecuteAsync(string appName, params string[] args)
        {
            this.process = new Process();
            this.appName = appName;

            this.process.StartInfo.FileName = this.appName;
            this.process.StartInfo.RedirectStandardError = true;
            this.process.StartInfo.StandardErrorEncoding = Encoding.UTF8;

            this.process.StartInfo.RedirectStandardInput = true;
            this.process.StartInfo.RedirectStandardOutput = true;
            this.process.EnableRaisingEvents = true;
            this.process.StartInfo.CreateNoWindow = true;

            this.process.StartInfo.UseShellExecute = false;

            this.process.StartInfo.StandardOutputEncoding = Encoding.UTF8;

            this.process.Exited += this.ProcessOnExited;
            if (this.Running)
            {
                throw new InvalidOperationException(
                    "Process is still Running. Please wait for the process to complete.");
            }

            string arguments = string.Join(" ", args);

            this.process.StartInfo.Arguments = arguments;

            this.context = SynchronizationContext.Current;

            this.process.Start();
            this.Running = true;

            new Task(this.ReadOutputAsync).Start();
            //new Task(this.WriteInputTask).Start();
            new Task(this.ReadOutputErrorAsync).Start();
        }

        public void Close()
        {
            if(process.HasExited)
            {
                return;
            }

            if(writer != null) writer.Close();
            process.Kill();
            process.WaitForExit();
            Running = false;
        }

        public void Write(string data)
        {
            if (data == null)
            {
                return;
            }

            lock (this.theLock)
            {
                this.pendingWriteData = data;
            }
        }

        public void WriteLine(string data)
        {
            if (Running == false)
                return;

            writer = process.StandardInput;
            writer.WriteLine(data);
            writer.Flush();
        }


        protected virtual void OnErrorTextReceived(string e)
        {
            EventHandler<string> handler = this.ErrorTextReceived;

            if (handler != null)
            {
                if (this.context != null)
                {
                    this.context.Post(delegate { handler(this, e); }, null);
                }
                else
                {
                    handler(this, e);
                }
            }
        }

        protected virtual void OnProcessExited()
        {
            EventHandler handler = this.ProcessExited;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        protected virtual void OnStandartTextReceived(string e)
        {
            EventHandler<string> handler = this.StandartTextReceived;

            if (handler != null)
            {
                if (this.context != null)
                {
                    this.context.Post(delegate { handler(this, e); }, null);
                }
                else
                {
                    handler(this, e);
                }
            }
        }

        private void ProcessOnExited(object sender, EventArgs eventArgs)
        {
            this.OnProcessExited();
        }

        private async void ReadOutputAsync()
        {
            var standart = new StringBuilder();
            var buff = new char[1024];
            int length;

            while (this.process.HasExited == false)
            {
                standart.Clear();

                length = await this.process.StandardOutput.ReadAsync(buff, 0, buff.Length);
                standart.Append(buff);
                this.OnStandartTextReceived(standart.ToString());
                Thread.Sleep(1);
            }

            this.Running = false;
        }

        private async void ReadOutputErrorAsync()
        {
            var sb = new StringBuilder();

            do
            {
                sb.Clear();
                var buff = new char[1024];
                int length = await this.process.StandardError.ReadAsync(buff, 0, buff.Length);
                sb.Append(buff);
                this.OnErrorTextReceived(sb.ToString());
                Thread.Sleep(1);
            }
            while (this.process.HasExited == false);
        }

        private async void WriteInputTask()
        {
            while (this.process.HasExited == false)
            {
                Thread.Sleep(1);

                if (this.pendingWriteData != null)
                {
                    await this.process.StandardInput.WriteLineAsync(this.pendingWriteData);
                    await this.process.StandardInput.FlushAsync();

                    lock (this.theLock)
                    {
                        this.pendingWriteData = null;
                    }
                }
            }
        }
    }
}