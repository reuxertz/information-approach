using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionTools
{
    public class DataSet
    {
        //Fields
        private object DataLock = new object(), IndexLock = new object();
        private int _currentIndex = 0;
        public List<string> Types = new List<string>();
        public List<Color> TColors = new List<Color>();
        private List<List<double[]>> Data = new List<List<double[]>>();

        //Properter
        public int CurrentIndex
        {
            get
            {
                return this._currentIndex;
            }
        }

        //Constructor
        public DataSet(string[] types)
        {
            //Add Types
            foreach (string s in types)
                this.AddType(s);
        }

        //Functions
        public void AddType(string s)
        {
            this.AddType(s, Color.White);
        }
        public void AddType(string s, Color c)
        {
            lock (this.DataLock)
            {
                //Create blank list at length
                var dataL = new List<double[]>();
                lock (this.IndexLock)
                {
                    for (int i = 0; i <= this._currentIndex; i++)
                        dataL.Add(new double[2] { 0, 0 });
                }

                //Add New Type
                this.Types.Add(s);
                this.TColors.Add(c);
                this.Data.Add(dataL);
            }
        }
        public void AddDataPoint(string type, double value, double weight)
        {
            lock (this.DataLock)
            {
                var found = false;
                for (int i = 0; i < this.Types.Count; i++)
                {
                    if (this.Types[i].Equals(type))
                    {
                        lock (this.IndexLock)
                        {
                            this.Data[i][this._currentIndex][0] += value;
                            this.Data[i][this._currentIndex][1] += weight;
                        }
                        found = true;
                    }
                }

                if (!found)
                {
                    this.AddType(type);
                    this.AddDataPoint(type, value, weight);
                    //this.Data[this.Data.Count - 1][currentIndex] = value;
                }
            }
        }
        public List<List<double[]>> DataCopy()
        {
            var rl = new List<List<double[]>>();
            lock (this.DataLock)
            {
                for (int i = 0; i < this.Data.Count; i++)
                {
                    rl.Add(new List<double[]>());
                    for (int j = 0; j < this.Data[i].Count; j++)
                    {
                        rl[i].Add(new double[2] { this.Data[i][j][0], this.Data[i][j][1] });
                    }
                }
            }
            return rl;
        }
        public void CompileInterval()
        {
            lock (this.DataLock)
            {
                for (int i = 0; i < this.Data.Count; i++)
                    this.Data[i].Add(new double[2]{ 0, 0 });

                lock (this.IndexLock)
                {
                    this._currentIndex++;
                }
            }
        }

    }
}
