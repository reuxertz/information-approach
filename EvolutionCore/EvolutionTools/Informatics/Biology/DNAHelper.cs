using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionTools.Bioinformatics
{
    public class DNAHelper
    {
        //private static fields
        private static string DNA = "ACGT";
        private static string GCbox = "GGGCGG";
        private static string CAATbox = "GCCCAATCT";
        private static string TATAbox = "TATAAA";

        //Static Sequence Functions
        public static string ConvertStringToRaw(String formattedSequence)
        {
            var seq = formattedSequence.ToUpper();
            var newSeq = new List<char>();

            for (int i = 0; i < seq.Length; i++)
                if (DNAHelper.DNA.Contains(seq[i]))
                    newSeq.Add(seq[i]);

            return new String(newSeq.ToArray<char>(), 0, newSeq.Count);
        }
        public static string InsertTags(string seq, int[] ATGstart, int[] ATGStop)
        {
            for (int i = 0; i < ATGstart.Length; i++)
            {
                String s1 = seq.Substring(0, ATGstart[i] - 1);
                String s2 = seq.Substring(ATGstart[i] + 2);

                String atg = seq.Substring(ATGstart[i] - 1, 3);

                if (!atg.ToUpper().Equals("ATG"))
                    throw new Exception("Invalid ATG position");

                seq = s1 + "<strt>" + atg + "</strt>" + s2;
            }


            for (int i = 0; i < ATGstart.Length; i++)
            {
                String s1 = seq.Substring(0, ATGstart[i]);
                String s2 = seq.Substring(ATGstart[i] + 3);

                String stop = seq.Substring(ATGstart[i], 3);

                //if (!atg.ToUpper().Equals("ATG"))
                //    throw new Exception("Invalid ATG position");

                seq = s1 + "<stp>" + stop + "</stp>" + s2;
            }

            return seq;
        }
    }
}
