using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NGD
{
    public sealed class EvoThreads : IDisposable
    {
        private readonly AutoResetEvent[]   _resume  = null;
        private readonly Thread[]           _threads = null;
        private readonly ManualResetEvent[] _waiting = null;

        private volatile Action<int> _body  = null;
        private volatile int         _bound = 0;
        private int                  _index = 0;
        private object               _lock  = new object();
        private volatile bool        _stop  = false;

        //private volatile bool _running = false;

        public EvoThreads()
            : this(Environment.ProcessorCount)
        {
        }

        public EvoThreads(int threads) : 
            this(threads, ThreadPriority.Normal)
        {
        }

        public EvoThreads(int threads, ThreadPriority priority)
        {
            // If the thread count is less than one or greater than the number of processors, throw an argument out of range exception
            if (threads < 1 || threads > Environment.ProcessorCount)
                throw new ArgumentOutOfRangeException("threads");

            // Initialize the threads array
            this._threads = new Thread[threads];

            // Initialize the event arrays
            this._resume = new AutoResetEvent[threads];
            this._waiting = new ManualResetEvent[threads];

            for (int i = 0; i < threads; i++)
            {
                // Create each thread in the threads array
                this._threads[i] = new Thread(new ParameterizedThreadStart(this.ThreadStart));

                //Set Priority
                this._threads[i].Priority = priority;

                // Set the properties for each thread in the threads array
                this._threads[i].IsBackground = true;

                // Create each element in the event arrays
                this._resume[i]  = new AutoResetEvent(false);
                this._waiting[i] = new ManualResetEvent(true);

                // Start each thread in the threads array
                this._threads[i].Start(i);
            }
        }

        public bool IsRunning
        {
            get
            {
                // If any thread in the threads array is not waiting, return true
                for (int i = 0; i < this._threads.Length; i++)
                    if (!this._waiting[i].WaitOne(0))
                        return true;

                return false;
            }
        }

        public void Break()
        {
            // Set the loop bound to the loop index
            this._bound = Thread.VolatileRead(ref this._index);
        }

        public void Dispose()
        {
            // Set the stop flag
            this._stop = true;

            for (int i = 0; i < this._threads.Length; i++)
            {
                // Abort each thread in the threads array
                this._threads[i].Abort();

                // Close each event in the event arrays
                this._resume[i].Close();
                this._waiting[i].Close();
            }
        }

        public void For(int fromInclusive, int toExclusive, Action<int> body)
        {
            lock (this._lock)
            {
                // Set the body delegate and loop bound
                this._body = body;
                this._bound = toExclusive - 1;

                // Set the loop index
                Interlocked.Exchange(ref this._index, fromInclusive - 1);

                for (int i = 0; i < this._threads.Length; i++)
                {
                    // Flag each thread in the threads array as running
                    this._waiting[i].Reset();

                    // Resume each thread in the threads array
                    this._resume[i].Set();
                }

                // Wait until each thread in the threads array is waiting
                for (int i = 0; i < this._threads.Length; i++)
                    this._waiting[i].WaitOne();
            }
        }

        public void Stop()
        {
            // Abort each thread in the threads array
            for (int i = 0; i < this._threads.Length; i++)
                this._threads[i].Abort();
        }

        private void ThreadStart(object obj)
        {
            // Get the thread index
            int i = (int)obj;

            // While the stop flag is not set
            while (!this._stop)
            {
                try
                {
                    // Wait until the thread is ready to resume
                    this._resume[i].WaitOne();

                    while (true)
                    {
                        //this._running = true;

                        // Get the index of the loop
                        int index = Interlocked.Increment(ref this._index);

                        // If the index of the loop has exceeded the bound, break the loop
                        if (index > this._bound)
                            break;

                        // Call the body delegate
                        this._body(index);
                    }

                    //this._running = false;
                }
                catch (ThreadAbortException exception)
                {
                    // ########## DEBUG ##########
                    Console.WriteLine(exception.ToString());

                    // If the stop flag is not set, reset the abort operation
                    if (!this._stop)
                        Thread.ResetAbort();
                }

                // Flag the thread as waiting
                this._waiting[i].Set();
            }
        }
    }
}
