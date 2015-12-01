using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

//using EvolutionGL;
using EvolutionTools;
using NGD;

namespace EvolutionTools
{
    using Point = EvolutionTools.Point;

    public class Simulator
    {
        public interface IAgent
        {
            //Functions
            void SetSimulator(Simulator s);
            bool NeedsRemoval
            {
                get;
                set;
            }
            bool IsRemoved
            {
                get;
                set;
            }
        }

        //Public Constants
        public int MinimumWaitTimeMilliseconds = 0; //volatile?

        //Private Fields Fields
        private Thread _mySimulationThread;
        private Parallel _parallel;
        private ID.Generator _regionIDGen = new ID.Generator();
        private List<IAgent> _addQueve = new List<IAgent>();//, _removeQueve = new List<Agent>();
        private Object _allRegionsLock = new Object(), _allAgentsLock = new Object(), _addQueveLock = new Object();//, _removeQueveLock = new Object();
        private Clock _myClock = new Clock(false, 0, 667);
        private bool _isDone, _isIterating, _performIterate, _abort, _performSingleIterate;//, _isMultiThreaded;

        //Protected Fields
        protected List<IAgent> _allAgents = new List<IAgent>();
        protected Data _myData;

        //Properties
        public List<IAgent> AllAgents
        {
            get
            {
                List<IAgent> rl = null;

                lock (this._allAgentsLock)
                {
                    rl = new List<IAgent>(this._allAgents);
                }

                return rl;
            }
        }
        public Data MyData
        {
            get
            {
                return this._myData;
            }
        }

        public bool IsDone
        {
            get
            {
                return _isDone;
            }
        }
        public bool IsIterating
        {
            get
            {
                return this._isIterating;
            }
        }
        public double CyclesPerSecond
        {
            get
            {
                return this._myClock.GetTicksPerElapsedTime(true, 0);
            }
        }
        public double CycleCount
        {
            get
            {
                return this._myClock.TotalTicks;
            }
        }

        public double TotalSugar = 0;

        //Construcotr
        public Simulator(int multiThreads)
        {
            //Create Thread to Run Simulation on
            this._mySimulationThread = new Thread(this._SimulationThreadHandler);

            //Create SubThreads for Multithreading if two or more
            if (multiThreads > 1)
            {
                //this._isMultiThreaded = true; ;
                //Create Arrays for threads
                this._parallel = new Parallel(multiThreads);
            }
        }

        //Operational Functions
        public void StartThread(bool startIteration)
        {
            this._mySimulationThread.Start();

            if (startIteration)
                this.StartIteration();
        }
        public void IterateSingle()
        {
            this._performSingleIterate = true;
        }
        public void StartIteration()
        {
            //Set performIterate bool which controls Thread control for iteration
            this._performIterate = true;

            //let Listeners know that the Universe thread is iterating             
            this._isIterating = true;
        }
        public void PauseIteration()
        {
            this._performIterate = false;
        }
        public void Abort()
        {
            //Stop Simulation thrad
            this._abort = true;

            //Wait for Simulation Thread to finish
            while (!this.IsDone) ;

            //End Parrallel
            //while (this._parallel.IsRunning) ;
            //this._parallel.Dispose();
        }

        //Methods
        public void AddAgent(IAgent a)
        {
            //Add to Universe
            lock (this._allAgentsLock)
                this._allAgents.Add(a);

            a.SetSimulator(this);

            lock (this._addQueveLock)
                this._addQueve.Add(a);

        }
        public void RemoveAgent(IAgent a)
        {
            //Add to Universe
            lock (this._allAgentsLock)
                this._allAgents.Remove(a);

            //if (removeFromRegions)
            a.IsRemoved = true;
        }
        public void HandleAgentChanges()
        {
            List<IAgent> toAdd = null;

            lock (this._addQueveLock)
            {
                toAdd = this._addQueve;
                this._addQueve = new List<IAgent>();
            }

            foreach (IAgent a in toAdd)
            {
                //foreach (Region r in this._myRegions)
                //{
                    //if (a.ParentRegion == null)
                    //    r.AddAgent(a);

                    //Add Agent to specific region based on certain criteria
                    //if (r.ContainsTileLocation(a.RegionTileLocation))
                    //{
                    //    r.AddAgent(a);
                    //    break;
                    //}
                //}
            }



        }

        public delegate void MultiThreadFunction(int i);

        //Thread Functions
        private void _SimulationThreadHandler()
        {
            //Begin Time
            this._myClock.Start();

            //Run Loop until abort is set to true
            //try
            {
                while (!this._abort)
                {
                    //Run while told to iterate
                    if (this._performIterate && (this.MinimumWaitTimeMilliseconds >= 0 || this._performSingleIterate))
                    {

                        if (this._performSingleIterate)
                            this._performSingleIterate = false;

                        this._isIterating = true;
                        //Get startTime
                        var start = DateTime.Now;

                        //////////////////////////////////////////
                        // Top Down (Universe) Pre Simulation  //
                        //////////////////////////////////////////

                        this._PreIterate();

                        //////////////////////////////////////////
                        // Region Handler / Multithread Handler //
                        // Bottom Up (Agent) Simulation         //
                        //////////////////////////////////////////

                        //this._ParallelIterate();

                        //////////////////////////////////////////
                        // Top Down (Universe) Post Simulation  //
                        //////////////////////////////////////////

                        this._PostIterate();

                        //////////////////////////////////////////
                        //////////////////////////////////////////

                        //Wait until final thread finished
                        //while (finalThread != null && finalThread.QuevedRegion != null) ;

                        //Wait if necessary
                        if (DateTime.Now.Subtract(start).Milliseconds < this.MinimumWaitTimeMilliseconds)
                            while (DateTime.Now.Subtract(start).Milliseconds < this.MinimumWaitTimeMilliseconds) ;

                        //Add Cycle Count
                        this._myClock.AddTick();

                    }
                    else
                    {
                        //Let listeneres know that the Universe is not running
                        if (this._isIterating)
                            this._isIterating = false;
                    }
                }

                //Let any Listeners know that the universe is done
                this._isIterating = false;
                this._isDone = true;
            }
        }
        private void _ParallelIterate(MultiThreadFunction mtf, int iterations)
        {
            //Handle Single Thread
            if (this._parallel == null)
            {
                this.HandleAgentChanges();

                for (int i = 0; i < iterations; i++)
                    mtf(i);

                this.HandleAgentChanges();
            }
            //Handle MultiThreaded
            else
            {
                this.HandleAgentChanges();

                this._parallel.For(0, iterations,
                    delegate(int i)
                    {
                        mtf(i);
                    });

                this.HandleAgentChanges();
            }
        }
        protected virtual void _PreIterate()
        {

        }
        protected virtual void _PostIterate()
        {

        }
    }
}