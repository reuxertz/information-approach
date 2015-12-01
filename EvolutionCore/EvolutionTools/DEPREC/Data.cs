using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionTools
{
    public class Data
    {
        //Fields
        private string[] _dataTypes;
        private List<double[]> _data = new List<double[]>();

        //Propperties
        public string[] DataTypes
        {
            get
            {
                return this._dataTypes;
            }
        }
        public List<double[]> DataCopy
        {
            get
            {
                var r = new List<double[]>();
                lock (_data)
                {
                    for (int i = 0; i < this._data.Count; i++)
                    {
                        lock (this._data[i])
                        {
                            var da = this._data[i];

                            var x = new double[da.Length];
                            for (int j = 0; j < da.Length; j++)
                                x[j] = da[j];
                            r.Add(x);
                        }
                    }
                }
                return r;
            }
        }

        //Constructor
        public Data(string[] dataTypes)
        {
            //Copy Data to dataTypes
            this._dataTypes = dataTypes;
            _data.Add(new double[this._dataTypes.Length]);
        }

        //Functions
        public void AddData(string type, double value)
        {
            //Find Match
            var found = false;
            for (int i = 0; i < this._dataTypes.Length; i++)
            {
                if (this._dataTypes[i] == type)
                {
                    this._data[this._data.Count - 1][i] += value;
                    found = true;
                    break;
                }
            }

            if (!found)
                throw new Exception("Match for data type not found within data object");


        }
        public void CompileIteration()
        {
            _data.Add(new double[this._dataTypes.Length]);


        }
        public void Export()
        {
            



        }
        


    }
}
