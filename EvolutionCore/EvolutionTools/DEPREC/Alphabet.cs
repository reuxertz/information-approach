using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionTools
{
    public class Alphabet
    {
        public class Info
        {
            protected int _sampleSize, _stateLength;
            protected List<string> _letters = new List<string>();
            protected List<double> _sampleLetterCounts = new List<double>();
            protected double[] _probabilitiesOfStates = null;
            protected double _entropyOfStates = -1, _entropyPerState = -1, _entropyDifFromMax = -1;

            public List<string> States
            {
                get
                {
                    return this._letters;
                }
            }
            public double[] StateProbabilities
            {
                get
                {
                    return this._probabilitiesOfStates;
                }
            }
            public int StateLength
            {
                get
                {
                    return this._stateLength;
                }
            }
            public double GetStateProbability(string state)
            {
                var st = new List<string>();
                bool multR = false, set = false;

                if (this._letters[0].Length < state.Length)
                {
                    if (state.Length % this._letters[0].Length == 0)
                    {
                        while (state.Length > 0)
                        {
                            st.Add(state.Substring(0, this._letters[0].Length));
                            state = state.Substring(this._letters[0].Length, state.Length - this._letters[0].Length);
                        }

                        multR = true;
                        set = true;
                    }
                    else
                        return -1;
                }

                if (!set && this._letters[0].Length == state.Length)
                {
                    st.Add(state);
                    multR = true;
                    set = true;
                }

                if (!set && this._letters[0].Length > state.Length)
                {
                    if (this._letters[0].Length % state.Length== 0)
                    {
                        foreach(string s in this._letters)
                        {
                            if (!s.Substring(0, state.Length).Equals(state))
                                continue;

                            st.Add(s);
                        }

                        set = true;
                    }
                    else
                        return -1;
                }

                var r = 0.0;
                if (multR)
                    r = -1.0;

                while (st.Count > 0)
                {
                    r = Math.Abs(r);
                    var index = this._letters.IndexOf(st[0]);
                    st.RemoveAt(0);

                    if (index == -1)
                        return -1;

                    if (multR)
                        r *= this._probabilitiesOfStates[index];
                    else
                        r += this._probabilitiesOfStates[index];
                }
                return r;
            }
            public List<string> GetOutputText(List<int> msgLengths)
            {
                var r = new List<string>();
                for (int i = 0; i < this._letters.Count; i++)
                {
                    var bpr = new double[this._letters.Count];

                    r.Add(this._letters[i] + ":");
                    for (int j = 0; j < msgLengths.Count; j++)
                    {
                        r[r.Count - 1] += "\t\t";
                        var sl = msgLengths[j];

                        if (this.StateLength < sl)
                        {
                            r[r.Count - 1] += "\t\t";
                        }
                        else if (this.StateLength == sl)
                        {
                            var si = "" + Math.Round(this._probabilitiesOfStates[i], 5);
                            var s = si.Substring(1, si.Length - 1);
                            r[r.Count - 1] += s;
                        }
                        else if (this.StateLength > sl)
                        {
                            r[r.Count - 1] += "\t\t";
                            var p = 0.0;
                                
                        }
                        else
                            r[r.Count - 1] += "\t\t";
                    }
                    //var si = "" + Math.Round(this._probabilitiesOfStates[i], 5);
                    //var s = this._letters[i] + ":\t\t" + si.Substring(1, si.Length - 1);
                    //r.Add(s);
                }

                return r;
            }

            //Constructor
            public Info(int stateLength)
            {
                this._stateLength = stateLength;
            }

            public void UpdateInfo()
            {
                this._probabilitiesOfStates = InfoMath.GetProbabilitiesOfStates(this._sampleLetterCounts.ToArray<double>(), this._sampleSize);
                this._entropyOfStates = -1 * InfoMath.GetTotalEntropyOfStates(this._probabilitiesOfStates);
                this._entropyPerState = this._entropyOfStates / this._letters.Count;
                this._entropyDifFromMax = this._entropyOfStates / Math.Log(this._letters.Count);
            }
            public void AddDiscreteSample(String sample, List<String> alpha = null)
            {
                if (sample.Length == 0)
                    throw new Exception();

                for (int i = 0; i < sample.Length; i++)
                    if (alpha != null && !alpha.Contains("" + sample[i]))
                        return;

                if (!this._letters.Contains(sample))
                {
                    this._letters.Add(sample);
                    this._sampleLetterCounts.Add(1);
                }
                else
                    this._sampleLetterCounts[_letters.IndexOf(sample)]++;

                this._sampleSize++;
            }
        }

        //Fields
        protected Info[] _letterInfo;


        //Properties
        public double GetStateProbability(string state)
        {
            for (int i = 0; i <this._letterInfo.Length; i++)
                if (this._letterInfo[i].StateLength == state.Length)
                    return this._letterInfo[i].GetStateProbability(state);

            return -1;
        }
        public Info[] MyInfo
        {
            get
            {
                return this._letterInfo;
            }
        }
        public string[] OutputString()
        {
            var r = new List<string>();
            r.Add("State");

            for (int i = 0; i < this.InfoStateLengths.Count; i++)
                r[0] += "\t\tProbability(n=" + this.InfoStateLengths[i] + ")";
            for (int i = 0; i < this._letterInfo.Length; i++)
            {
                r.AddRange(this._letterInfo[i].GetOutputText(this.InfoStateLengths));
                r.Add(" ");
            }

            return r.ToArray<string>();
        }
        public List<int> InfoStateLengths
        {
            get
            {
                var r = new List<int>();
                for (int i = 0; i < this._letterInfo.Length; i++)
                    r.Add(this._letterInfo[i].StateLength);
                return r;
            }
        }

        //Constructors
        public Alphabet()
        {
            this._letterInfo = new Info[] { new Info(1) };
        }
        public Alphabet(string[] sample, string alpha) : this()
        {
            if (sample.Length == 0)
                throw new Exception();

            var a = new List<String>();
            for (int i = 0; i < alpha.Length; i++)
                a.Add("" + alpha[i]);

            for (int i = 0; i < sample.Length; i++)
                for (int j = 0; j < sample[i].Length; j++)
                    this._letterInfo[0].AddDiscreteSample("" + sample[i][j], a);

            this._letterInfo[0].UpdateInfo();
        }
        public Alphabet(String[][] sample, List<String> alpha) : this()
        {
            if (sample.Length == 0)
                throw new Exception();

            for (int i = 0; i < sample.Length; i++)
                for (int j = 0; j < sample[i].Length; j++)
                    this._letterInfo[0].AddDiscreteSample(sample[i][j], alpha);

            this._letterInfo[0].UpdateInfo();
        }
        public Alphabet(int[] orders)
        {
            var r = new Info[orders.Length];
            for (int i = 0; i < r.Length; i++)
                r[i] = new Info(orders[i]);
            this._letterInfo = r;
        }

        //Functions
        public void AddMessageSample(String sample, List<String> alpha)
        {
            for (int i = 0; i < sample.Length; i++)
                for (int j = 0; j < this._letterInfo.Length; j++)
                    if (i + this._letterInfo[j].StateLength < sample.Length)
                        this._letterInfo[j].AddDiscreteSample(sample.Substring(i, this._letterInfo[j].StateLength), alpha);
        }
        public void AddMessageSamples(List<String> sample, List<String> alpha)
        {
            for (int i = 0; i < sample.Count; i++)
                this.AddMessageSample(sample[i], alpha);
        }

        //Creators
        public static Alphabet AlphabetByOrder(string[] sample, string alpha, int bitLength)
        {
            if (sample.Length == 0)
                throw new Exception();

            var a = new List<String>();
            int[] ii = new int[bitLength];
            while (true)
            {
                var s = "";
                for (int i = 0; i < bitLength; i++)
                    s += alpha[ii[i]];

                a.Add(s);
                ii[0]++;

                if (ii[0] < alpha.Length)
                    continue;

                int index = 0;
                bool done = false, cont = false;
                while (ii[index] >= alpha.Length)
                {
                    ii[index] = 0;

                    if (index + 1 >= ii.Length)
                    {
                        done = true;
                        break;
                    }

                    ii[index + 1]++;

                    if (ii[index + 1] >= alpha.Length)
                        index++;
                    else
                    {
                        cont = true;
                        break;
                    }
                }

                if (cont)
                    continue;

                if (done)
                    break;

                break;
            }

            var r = new Alphabet();
            for (int i = 0; i < sample.Length; i++)
                for (int j = 0; j < sample[i].Length - bitLength; j++)
                {
                    var s = "";                      
                    for (int k = 0; k < bitLength; k++)
                        s += sample[i][j + k];
                    r.AddMessageSample(s, a);
                }

            r.UpdateInfo();
            return r;
        }

        //Functions
        public void UpdateInfo()
        {
            for (int i = 0; i < this._letterInfo.Length; i++)
                this._letterInfo[i].UpdateInfo();
        }
    }
}
