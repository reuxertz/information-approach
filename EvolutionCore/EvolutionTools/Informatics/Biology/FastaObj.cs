using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionTools
{
    public class FastaObj
    {
        public static string CreateFastaHeader(int index, String[] fastaTxt)
        {
            return FastaObj.CreateFastaHeader(index, fastaTxt.Length, fastaTxt);
        }
        public static string CreateFastaHeader(int index, int endIndex, String[] fastaTxt)
        {
            int curRowIndex = index;
            while (curRowIndex < fastaTxt.Length)
            {
                int startIndex = fastaTxt[curRowIndex].IndexOf(@">", 0);
                if (startIndex == -1)
                {
                    curRowIndex++;
                    continue;
                }

                int i = 1;
                for (i = 1; i + curRowIndex <= fastaTxt.Length; i++)
                    if (fastaTxt[i + curRowIndex].Contains("]") || i + curRowIndex > endIndex)
                        break;
                    else
                        continue;
                var header = "";
                for (int j = 0; j < i; j++)
                    header += fastaTxt[curRowIndex + j];

                return header;

                //curRowIndex++;
                //continue;
            }

            return null;
        }
        public static void CreateMultiFastaFile(String path, List<FastaObj> objs)
        {
            var txt = new List<String>();
            for (int i = 0; i < objs.Count; i++)
            {
                txt.Add(objs[i].rawFasta);
                txt.Add(objs[i].sequence);
            }

            Management.WriteTextToFile(path, txt.ToArray<string>(), false);
        }
        public static void CreateMultiFastaFile(String path, List<List<FastaObj>> objs)
        {
            var txt = new List<String>();
            for (int i = 0; i < objs.Count; i++)
            {
                for (int j = 0; j < objs[i].Count; j++)
                {
                    txt.Add(objs[i][j].rawFasta);
                    txt.Add(objs[i][j].sequence);
                }
            }

            Management.WriteTextToFile(path, txt.ToArray<string>(), false);
        }
        public static void CreateCSVFromMultiFastaFile(string path)
        {
            string newpath = path.Replace(".fa", ".csv");

            var txt = Management.GetTextFromFile(path);

            var rf = new List<string>();
            var rs = new List<string>();
            for (int i = 0; i < txt.Length; i++)
            {
                if (txt[i].Contains(">"))
                {
                    rf.Add("");
                    while (!txt[i].Contains("]"))
                    {
                        if (i >= txt.Length)
                            break;

                        rf[rf.Count - 1] += (txt[i].Trim());
                        i++;
                    }

                    if (i >= txt.Length)
                        break;

                    var i2 = txt[i].IndexOf("]");
                    rf[rf.Count - 1] += (txt[i].Substring(0, i2 + 1).Trim());
                    rs.Add(txt[i].Substring(i2, (txt[i].Length - i2) - 1).Trim());
                    i++;

                    if (i >= txt.Length)
                        break;
                    
                    while (!txt[i].Contains(">"))
                    {
                        rs[rs.Count - 1] += (txt[i].Trim());
                        i++;

                        if (i >= txt.Length)
                            break;
                    }

                    if (i >= txt.Length)
                        break;

                    var i3 = txt[i].IndexOf(">");
                    rs[rs.Count - 1] += (txt[i].Substring(0, i3).Trim());

                }
            }

            if (rf.Count != rs.Count)
                return;

            var rt = new List<String>();
            for (int i = 0; i < rf.Count; i++)
                rt.Add(rf[i] + "," + rs[i]);

            Management.WriteTextToFile(newpath, rt, false);
            return;
        }

        public int giID;
        public int? sourceIndex;
        public string rawFasta, info, species, sequence;
        public ArCOGObj arCOG;
        public EntropyProfile entropyProfile;

        public List<float> getSequenceProfileSelf()
        {
            return this.getSequenceProfile(this.entropyProfile.states, this.entropyProfile.statesProbabilities,
                this.entropyProfile.stateFrameWidth, this.entropyProfile.stateVelocity);
        }
        public List<float> getSequenceProfile(List<string> states, List<float> probabilities, int width, int velocity)
        {
            var r = new List<float>();

            var tv = -1.0f;
            var f = .66f;
            for (int i = 0; i <= sequence.Length - width; i += velocity)
            {
                var pv = probabilities[states.IndexOf(sequence.Substring(i, width))];

                if (i == 0)
                    tv = (float)400 * pv;
                else
                    tv = (tv * f) + (400 * pv * (1 - f));

                r.Add(tv);
            }

            return r;
        }

        public FastaObj updateEntropyProfile(int width, int velocity)
        {
            this.entropyProfile = new EntropyProfile(this.sequence, ProteinHelper.AminoAcidCharStrings, width, velocity);

            return this;
        }

        public FastaObj(String fasta, String sequence)
            : this(fasta, sequence, -1)
        { }
        public FastaObj(String fasta, String sequence, int sourceIndex)
        {
            this.rawFasta = fasta;
            if (sequence != null)
                this.sequence = sequence.Trim();
            this.sourceIndex = sourceIndex;

            var splFasta = fasta.Split('|');
            var splFastaInfo = splFasta[4].Split('[');

            this.giID = Convert.ToInt32(splFasta[1].Trim());
            this.info = splFastaInfo[0].Trim();
            this.species = splFastaInfo[1].Trim().Replace("]", "");

            return;
        }

        public string[] CreateFastaArray()
        {
            return new string[] { this.rawFasta, this.sequence };
        }
        public void CreateLocalFasta(string path)
        {
            Management.WriteTextToFile(path, new string[] { this.rawFasta, this.sequence }, false);
        }
    }
}
