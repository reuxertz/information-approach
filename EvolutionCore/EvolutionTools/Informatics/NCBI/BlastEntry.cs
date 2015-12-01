using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionTools
{
    public class BlastEntry
    {
        protected static String[] gatherStrings = new String[] { "Score", "Expect", "Identities" };

        public string rawHeader, rawInfo;
        public FastaObj blastFasta;//, sourceFasta;
        public double Score, EValue, Identity;

        public BlastEntry(string header, string info)
        {
            this.rawHeader = header;
            this.rawInfo = info;

            var index = info.IndexOf("Score");
            var endIndex = info.IndexOf(",", index);

            var ss1 = info.Substring(index, endIndex - index).Replace(" ", "");
            var ts1 = ss1.IndexOf('=') + 1;
            var scoreSTR = ss1.Substring(ts1, ss1.IndexOf('b') - ts1);
            var score = Convert.ToDouble(scoreSTR);

            index = info.IndexOf("Expect");
            endIndex = info.IndexOf(",", index);

            ss1 = info.Substring(index, endIndex - index).Replace(" ", "");
            ts1 = ss1.IndexOf('=') + 1;
            var evalueSTR = ss1.Substring(ts1, ss1.Length - ts1);
            var eindex = evalueSTR.IndexOf('e');

            var evalue = 0.0;
            if (eindex != -1)
            {
                var v1 = evalueSTR.Substring(0, eindex);
                var v2 = evalueSTR.Substring(eindex + 1);
                evalue = Convert.ToDouble(v1) * Math.Pow(10, Convert.ToDouble(v2));
            }
            else
            {
                evalue = Convert.ToDouble(evalueSTR);
            }

            index = info.IndexOf("Identities");
            endIndex = info.IndexOf(",", index);

            ss1 = info.Substring(index, endIndex - index).Replace(" ", "");
            ts1 = ss1.IndexOf('(') + 1;
            var identity = Convert.ToDouble(ss1.Substring(ts1, ss1.IndexOf("%") - ts1)) / 100.0;

            this.blastFasta = new FastaObj(header, null);

            this.Score = score;
            this.EValue = evalue;
            this.Identity = identity;

            return;
        }
    }
}
