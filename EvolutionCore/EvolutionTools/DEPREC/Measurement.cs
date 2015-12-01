using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionTools
{
    public class Clock
    {
        //Fields
        private DateTime _startTime, _stopTime;
        private TimeSpan? _currentElapsedTime = new TimeSpan();
        private double _mostRecentTicksPerTime = -1;
        private bool _isRunning = false, _resetTicks = false;
        private int _totalTicks, _ticks = 0, _minimumIntervalMilliseconds = 1;                               //Volatile?

        //Properties
        public int Ticks
        {
            get
            {
                return this._ticks;
            }
        }
        public int TotalTicks
        {
            get
            {
                return this._totalTicks;
            }
        }

        //Constrcucotr
        public Clock()
        { }
        public Clock(bool start, int count, int minIntervalInMilliseconds)
        {
            this._ticks = count;
            this._minimumIntervalMilliseconds = minIntervalInMilliseconds;

            if (start)
                this.Start();
        }

        //Functions
        public TimeSpan GetCurrentElapsedTime(bool resetInterval)
        {
            //Handle Error
            if (!this._isRunning)
                throw new Exception("Clock not running");

            //Get Elapsed Time
            DateTime now = DateTime.Now;
            this._currentElapsedTime = now.Subtract(this._startTime);

            //Reset StartTime
            if (resetInterval)
                this._startTime = now;

            return this._currentElapsedTime.Value;
        }
        public double GetTicksPerElapsedTime(bool resetInterval, int round)
        {
            //Handle Error
            if (!this._isRunning)
                return 0;

            //Get Elapsed Time and Tick INterval
            DateTime now = DateTime.Now;
            TimeSpan currentElapsedTime = now.Subtract(this._startTime);

            //If Minimum time has not been reached, return
            if (currentElapsedTime.Milliseconds < this._minimumIntervalMilliseconds)
            {
                if (this._mostRecentTicksPerTime == -1)
                    return 0;

                return this._mostRecentTicksPerTime;
            }

            //Set Fields
            this._currentElapsedTime = currentElapsedTime;

            //Reset Ticks
            if (this._resetTicks)
            {
                this._ticks = 0;
                this._resetTicks = false;
            }

            //Get TicksPerTime
            double ticksPerTime = (this._ticks * 1000) / this._currentElapsedTime.Value.TotalMilliseconds;        

            //Reset StartTime
            if (resetInterval)
            {
                this._startTime = now;
                this._resetTicks = true;
            }

            if (round >= 0)
                ticksPerTime = Math.Round(ticksPerTime, round);

            this._mostRecentTicksPerTime = ticksPerTime;  

            return ticksPerTime;
        }
        
        //Methods
        public void AddTick(int count)
        {
            if (this._resetTicks)
            {
                this._ticks = 0;
                this._resetTicks = false;
            }

            this._ticks += count;
            this._totalTicks += count;
        }
        public void AddTick()
        {
            if (this._resetTicks)
            {
                this._ticks = 0;
                this._resetTicks = false;
            }

            this._ticks++;
            this._totalTicks++;
        }        
        public void Start()
        {
            //If already Running return
            if (this._isRunning)
                throw new Exception("Clock already running");

            //Get Current Time And Put as Start
            this._startTime = DateTime.Now;

            //Set object properties
            this._isRunning = true;
        }
        public void Stop()
        {
            //Handle Error
            if (!this._isRunning)
                throw new Exception("Clock not running");

            //Get Time Elapsed
            this._stopTime = DateTime.Now;

            //Set Object properties
            this._isRunning = false;

            //Get Time Between Start and Stop
            this._currentElapsedTime = this._stopTime.Subtract(this._startTime);

            return;
        }      
    }
    public class StopWatch
    {
        //Fields
        private DateTime _startTime;
        private TimeSpan? _currentElapsedTime = new TimeSpan();
        private bool _isRunning = false;

        //Properties
        public TimeSpan CurrentElapsedTime
        {
            get
            {
                if (this._currentElapsedTime != null)
                    return new TimeSpan(this._currentElapsedTime.Value.Ticks);

                if (this._isRunning)
                    return this._startTime.Subtract(DateTime.Now);

                throw new Exception("Unhandled Region");
            }
        }

        //Constructor
        public StopWatch()
        { }

        //Functions
        public void Start()
        {
            //If already Running return
            if (this._isRunning)
                throw new Exception("Clock already running");

            //Get Current Time And Put as Start
            this._currentElapsedTime = null;
            this._startTime = DateTime.Now;

            //Set object properties
            this._isRunning = true;
        }
        public void Stop()
        {
            //Handle Return
            if (!this._isRunning)
                throw new Exception("Clock not running");

            //Get Time Elapsed
            DateTime stopTime = DateTime.Now;

            //Set Object properties
            this._isRunning = false;

            //Get Time Between Start and Stop
            this._currentElapsedTime = stopTime.Subtract(this._startTime);

            return;
        }
        public void Restart()
        {
            //Stop if Running
            if (this._isRunning)
                this.Stop();

            //Start
            this.Start();
        }
    }
    public class ID
    {
        //Generator Class
        protected class Generator
        {
            //fields
            private Counter _myCounter;

            //Functions
            public int NextID()
            {
                if (this._myCounter.Count == int.MaxValue)
                    throw new Exception("integer limit reached");
                
                var r = this._myCounter.Count;
                this._myCounter.AddCount();
                return r;
            }

        }

        //Public Fields
        public object Tag;

        //Fields
        private Generator _myIDGenerator;
        private ID[] _Parents;
        private int _ID;

        //Prpoerites
        public int IDNum
        {
            get
            {
                return this._ID;
            }
        }

        //Consturctor
        public ID()
        {
            this._myIDGenerator = new Generator();
            this._ID = this._myIDGenerator.NextID();
        }
        public ID(ID parent)
        {
            this._myIDGenerator = parent._myIDGenerator;
            this._ID = this._myIDGenerator.NextID();
            this._Parents = new ID[1] { parent };
        }
        public ID(ID parent1, ID parent2)
        {
            if (parent1._myIDGenerator != parent2._myIDGenerator)
                throw new Exception("Cannot have parents with differing ID pools");

            this._myIDGenerator = parent1._myIDGenerator;
            this._ID = this._myIDGenerator.NextID();
            this._Parents = new ID[2] { parent1, parent2 };
        }     
    }
    public class GenIDXXX
    {
        //Static Functions
        public static bool Match(int[] ID1, int[] ID2)
        {
            if (ID1.Length != ID2.Length)
                return false;

            for (int i = 0; i < ID1.Length; i++)
                if (ID1[i] != ID2[i])
                    return false;

            return true;
        }
        public static bool Match(string ID1, string ID2, char sep)
        {
            if (ID1.Length != ID2.Length)
                return false;

            for (int i = 0; i < ID1.Length; i++)
                if (ID1[i] != ID2[i])
                    return false;

            return true;
        }
        public static string IDIntToString(int[] id, char sep)
        {
            var s = "";

            for (int i = 0; i < id.Length; i++)
            {
                s += (id[i]);
                    
                if (i < id.Length - 1)
                    s += sep;
            }

            return s.Substring(0, s.Length - 2);


        }
        public static int[] IDStringToInt(string id, char sep)
        {
            var splID = id.Split(sep);
            var IDl = new List<int>();
            for (int i = 0; i < splID.Length; i++)
                if (splID[i] != "")
                    IDl.Add(Convert.ToInt32(splID[i]));

            return IDl.ToArray<int>();
        }
        public static int[] GenerationDifference(GenIDXXX id1, GenIDXXX id2)
        {
            var lim = Math.Min(id1._ID.Length, id2._ID.Length);
            for (int i = 0; i < lim; i++)
            {
                if (id1._ID[i] != id2._ID[i])
                {
                    return new int[2] { id1._ID.Length - i, id2._ID.Length - i };
                }
            }

            throw new NotImplementedException();
        }

        //Private Fields
        private int[] _ID;
        private int _offspringCount = 0;

        //Prpoerites
        public int[] IDNum
        {
            get
            {
                return this._ID.ToArray<int>();
            }
        }
        public string IDString
        {
            get
            {
                return GenIDXXX.IDIntToString(this._ID, '.');
            }
        }

        //Consturctor
        public GenIDXXX()
        {
            this._ID = new int[1];
            this._ID[0] = 0;
        }
        public GenIDXXX(GenIDXXX parent)
        {
            var idl = parent._ID.ToList<int>();
            idl.Add(parent._offspringCount);
            this._ID = idl.ToArray<int>();

            parent._offspringCount++;
        }
    }

    public struct Counter
    {
        //Fields
        private int _count;

        //Properties
        public int Count
        {
            get
            {
                return this._count;
            }
        }

        //Constrcuctor
        public Counter(int count)
        {
            this._count = count;
        }

        //Functions
        public void AddCount(int count)
        {
            this._count += count;
        }
        public void AddCount()
        {
            this._count++;
        }
        public void Reset()
        {
            this._count = 0;
        }
    }
}
