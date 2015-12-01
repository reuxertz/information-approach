using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace EvolutionTools
{
    public class RunCommand
    {
        protected System.ComponentModel.IContainer components = new System.ComponentModel.Container();
        protected string _currentWorkDirectory = "";
        protected List<string> _reqs = new List<string>(), _workDirs = new List<string>(),
            _outputStrings = new List<string>(), _newReqQueve = new List<string>();
        protected List<DateTime> _outputTime = new List<DateTime>();
        protected List<int> _outputReqIndexes = new List<int>();
        protected bool _listenForOutput = false, _markKill = false, _isRunning = false;
        protected Process _process = null;

        public delegate void CommandEvent(RunCommand rc, String msg);

        public event CommandEvent OnNewLine;
        public event CommandEvent OnLastLine;

        public string WorkingDirectory
        {
            get
            {
                return this._currentWorkDirectory;
            }
            set
            {
                if (Directory.Exists(value))
                    this._currentWorkDirectory = value;
            }
        }
        public Boolean isListening
        {
            get
            {
                return this._listenForOutput;
            }
        }
        public Boolean isRunning
        {
            get
            {
                return this._isRunning;
            }
        }

        public void AddRequest(String req)
        {
            lock (this._newReqQueve)
            {
                this._newReqQueve.Add(req);
            }

            if (!this.isListening)
                this._HandleNextRequest();
        }
        public void AddRequest(String req, String wd)
        {
            this.WorkingDirectory = wd;
            this.AddRequest(req);
        }

        public RunCommand()
        {
        }
        public RunCommand(String workingDirectory)
            : this()
        {
            this._currentWorkDirectory = workingDirectory;
        }

        protected void _HandleNextRequest()
        {
            if (this._newReqQueve.Count > 0)
            {
                String nr = null;
                lock (this._newReqQueve)
                {
                    nr = this._newReqQueve[0];
                    this._newReqQueve.RemoveAt(0);
                }
                this.RunCommandLine(nr);
            }
        }
        protected void _HandleCommandLineOutput(DataReceivedEventArgs e, string state)
        {
            if (!this._listenForOutput)
            {
                this._isRunning = false;
                return;
            }

            //if (e.Data == null)

            this._outputStrings.Add(e.Data);
            this._outputReqIndexes.Add(this._reqs.Count);
            this._outputTime.Add(DateTime.Now);

            String data = e.Data;
            //if (data == null)
            //   data = "";

            if (data == null || (data.Contains(@":\") && data.Contains(@">")))
            {
                this._listenForOutput = false;

                if (data != null)
                    this._currentWorkDirectory = data.Substring(0, data.IndexOf('>'));

                if (this.OnLastLine != null)
                    this.OnLastLine(this, data);
                this._process = null;

                if (!this._markKill)
                {
                    this._HandleNextRequest();
                }

                this._isRunning = false;
                return;
            }
            else if (this.OnNewLine != null)
                this.OnNewLine(this, e.Data);

        }
        public void Kill()
        {
            this._markKill = true;
            if (this._process != null && !this._process.HasExited)
                this._process.Kill();
        }
        public void RunCommandLine(string req)
        {
            this.RunCommandLine(req, this._currentWorkDirectory);
        }
        public string RunCommandLine(string req, string wd)
        {
            //this._isDone = false;
            this._listenForOutput = true;

            if (wd == null || wd == "")
            {
                return "error - invalid command directory";
            }

            //using (
            Process p = this._process = new Process();
            //)
            //{
            p.StartInfo = new ProcessStartInfo("cmd.exe")
            {
                //RedirectStandardInput = true,
                RedirectStandardOutput = true,
                //RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = wd,
                Arguments = "/K " + req,
                CreateNoWindow = true
            };
            // event handlers for output & error
            p.OutputDataReceived += (sender, e) => this._HandleCommandLineOutput(e, "Output");
            //p.ErrorDataReceived += (sender, e) => this._HandleCommandLineOutput(e, "Error");

            // start process
            this._reqs.Add(req);
            this._workDirs.Add(wd);
            p.Start();
            this._isRunning = true;
            // send command to its input
            //p.StandardInput.Write(req + p.StandardInput.NewLine);
            p.BeginOutputReadLine();
            //p.BeginErrorReadLine();
            //wait
            //p.WaitForExit();

            //if (p != null && !p.HasExited)
            //    p.Kill();
            //}

            return "command execution success";
        }
    }
}
