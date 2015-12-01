using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace EvolutionTools
{
    public class BlastObj
    {
        public FastaObj queryFasta;
        public List<BlastEntry> entries = new List<BlastEntry>();

        public BlastObj(string queryPath, string blastTextPath)
        {
            var qTxt = Management.GetTextFromFile(queryPath);
            var bTxt = Management.GetTextFromFile(blastTextPath);

            this.queryFasta = new FastaObj(qTxt[0], qTxt[1]);

            var btxtL = bTxt.ToList<String>();
            var changed = false;
            for (int i = 0; i < btxtL.Count; i++)
            {
                var cnt = 0;
                var ns = new List<String>();
                for (int j = 0; j < btxtL[i].Length; j++)
                {
                    if (btxtL[i][j] == '>')
                    {
                        cnt++;

                        if (cnt == 1)
                            continue;

                        changed = true;

                        ns.Add(btxtL[i].Substring(0, j));
                        btxtL[i] = btxtL[i].Substring(j);
                        j = 0;
                    }

                    continue;
                }

                while (ns.Count > 0)
                {
                    btxtL.Insert(i + 1, ns[0]);
                    ns.RemoveAt(0);
                }
            }

            if (changed)
                bTxt = btxtL.ToArray<string>();
            
            int curRowIndex = 0;
            while (curRowIndex < bTxt.Length)
            {
                int startIndex = bTxt[curRowIndex].IndexOf(@">", 0);
                if (startIndex == -1)
                {
                    curRowIndex++;
                    continue;
                }

                int i = 1, headerEndIndex = -1, fastaEndIndex = -1;
                for (i = 1; i + curRowIndex <= bTxt.Length; i++)
                {
                    if ((headerEndIndex != -1 && fastaEndIndex != -1))
                        break;

                    if (i + curRowIndex >= bTxt.Length)
                    {
                        if (headerEndIndex != -1 && fastaEndIndex == -1)
                            fastaEndIndex = i + curRowIndex - 1;
                        break;
                    }

                    if (headerEndIndex == -1 && bTxt[i + curRowIndex].Contains("]"))
                        headerEndIndex = i + curRowIndex;

                    if (fastaEndIndex == -1 && headerEndIndex != -1 && bTxt[i + curRowIndex].Contains(">"))
                        fastaEndIndex = (i - 1) + curRowIndex;
                    
                    continue;
                }
                if (headerEndIndex == -1 || fastaEndIndex == -1)
                    break;

                var header = "";
                var fastaInfo = "";

                for (i = curRowIndex; i <= headerEndIndex; i++)
                    if (bTxt[i].Contains('>'))
                        header += bTxt[i].Substring(bTxt[i].IndexOf(">"));
                    else
                        header += bTxt[i];

                for (i = headerEndIndex + 1; i <= fastaEndIndex; i++)
                    fastaInfo += bTxt[i];

                var hei = header.IndexOf("Length=");
                if (hei != -1 && hei + 1 < header.Length)
                {
                    fastaInfo = header.Substring(hei) + fastaInfo;
                    header = header.Substring(0, hei);
                }


                var be = new BlastEntry(header, fastaInfo);

                this.entries.Add(be);

                curRowIndex++;
                continue;
            }

            /*
            var x = 0;
            var ids = new List<int>();
            foreach (String s in bTxt)
                foreach (Char c in s)
                {
                    if (c == '>')
                    {
                        x++;

                        var ind = s.IndexOf("|", 5);
                        var ts = s.Substring(5, ind - 5);
                        ids.Add(Convert.ToInt32(ts));
                    }

                }

            var ids2 = new List<int>();
            foreach (BlastEntry be in this.entries)
                ids2.Add(be.fastaInfo.giID);       
            foreach(int id in ids)
                if (!ids2.Contains(id))
                {
                    continue;
                }*/

            return;
        }

        /*public void CreateMultiFastaFile(String path)
        {
            var txt = new List<string>();

            foreach (BlastEntry be in this.entries)
            {
                if (be.sourceFasta == null)
                    continue;

                txt.AddRange(be.sourceFasta.CreateFastaArray());
            }

            Management.WriteTextToFile(path, txt.ToArray<string>(), false);
        }*/
    }
}
