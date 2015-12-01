using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionTools
{
    public static class InfoMath
    {
        public static string[] SequentializeSample(string[] smpls, bool addEndSpaces, bool removeCase, bool removeSpace, bool removeOther)
        {
            for (int i = 0; i < smpls.Length; i++)
            {
                if (removeCase)
                    smpls[i] = smpls[i].ToUpperInvariant();

                if (removeSpace)
                    smpls[i].Replace(" ", "");

                if (removeOther)
                    smpls[i].Replace("\\n", "");

                if (addEndSpaces)
                {
                    if (smpls[i][0] != ' ')
                        smpls[i] = " " + smpls[i];
                    if (smpls[i][smpls[i].Length - 1] != ' ')
                        smpls[i] += " ";
                }

            }

            return smpls;
        }
        public static double[] GetProbabilitiesOfStates(double[] counts, double total)
        {
            var r = new double[counts.Length];
            for (int i = 0; i < r.Length; i++)
                r[i] = counts[i] / total;

            return r;
        }
        public static double GetTotalEntropyOfStates(double[] probs)
        {
            var r = 0.0;
            for (int i = 0; i < probs.Length; i++)
                r += probs[i] * Math.Log(probs[i]);

            return r;
        }
    }
}
