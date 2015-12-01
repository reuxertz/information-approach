using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EvolutionTools
{
    public class CommandLibrary
    {
        public class StateSequence : CommandConsole.ArgClass
        {
            //Static fields
            public static string[] AlphabetTypes    = new string[] { "DNA", "EnglishBasic" };
            public static string[] Alphabets        = new string[] { "ACGT", " ABCDEFGHIJKLMNOPQRSTUVWXYZ." };
            public static string[] ValidFileTypes   = new string[] { "txt", "fasta" };

            //Static Functions
            public static string RemoveInvalidAlphas(string seq, string validAlpha)
            {
                if (validAlpha == null)
                    return seq;

                var r = "";
                for (int i = 0; i < seq.Length; i++)
                {
                    if (validAlpha.Contains(seq[i]))
                    {
                        var t = seq[i];
                        r += t;
                    }
                }
                return r;
            }
            public static void FastaFileToArrays(string[] f, out List<string> fasta, out List<string> seqs)
            {
                fasta = new List<string>();
                seqs = new List<string>();
                
                bool startFound = false, endStartFound = false;
                for (int i = 0; i < f.Length; i++)
                {
                    //f[i] = f[i].Replace(@", ", "; ");
                    //f[i] = f[i].Replace(@",", "-");

                    if (f[i].Contains(">"))
                    {
                        endStartFound = false;
                        fasta.Add(f[i].Trim());
                        seqs.Add("");
                        //o.Add(f[i].Trim());
                        startFound = true;
                        endStartFound = true;

                        continue;
                    }

                    if (startFound & !endStartFound)
                    {
                        if (!f[i].Contains("]"))
                        {
                            fasta[fasta.Count - 1] += f[i].Trim();
                            continue;
                        }
                        else
                        {
                            var ind = f[i].IndexOf("]");
                            fasta[fasta.Count - 1] += f[i].Substring(0, ind).Trim();
                            seqs[seqs.Count - 1] += f[i].Substring(ind, f[i].Length - ind).Trim();
                            endStartFound = true;
                            continue;
                        }
                    }

                    if (endStartFound)
                    {
                        if (!f[i].Contains(">"))
                        {
                            seqs[seqs.Count - 1] += f[i].Trim();
                            continue;
                        }
                        else
                        {
                            i--;
                            startFound = false;
                            continue;
                        }
                    }
                }
            }
            public static List<string> FastaToCSV(string path)
            {
                List<string> fasta, seqs, r = new List<string>();

                StateSequence.FastaFileToArrays(Management.GetTextFromFile(path), out fasta, out seqs);

                for (int i = 0; i < fasta.Count; i++)
                    r.Add(fasta[i] + "," + seqs[i]);

                return r;
            }

            public class SeqRef : CommandConsole.ArgClass.Reference
            {
                //Fields
                protected List<string> _allSeqs = null, _allFasta = null;
                protected Alphabet _myAlpha = new Alphabet(new int[] { 1, 2, 3 });

                //Properties
                public List<string> AllSeqs
                {
                    get
                    {
                        return this._allSeqs;
                    }
                }
                public List<string> AllFasta
                {
                    get
                    {
                        return this._allFasta;
                    }
                }
                public string[] OutputText
                {
                    get
                    {
                        return this._myAlpha.OutputString();
                    }
                }
                public Alphabet MyAlphabet
                {
                    get
                    {
                        return this._myAlpha;
                    }
                }

                //Override Functions
                public override bool? ExecuteCall(CommandConsole c, Reference existRef, CommandConsole.ReturnArgs retArgs, string[] swtchs, List<string> outputFeed)
                {
                    if (this._argName == null)
                        return this.Parent.ExecuteCall(c, existRef, retArgs, swtchs, outputFeed);

                    return false;
                }
                public override bool? AttemptConstruct(CommandConsole c, Reference r, CommandConsole.ReturnArgs retArgs, string[] sws, List<string> outputFeed)
                {
                    var a = (string)null;
                    if (sws.Length > 0)
                        for (int i = 0; i < AlphabetTypes.Length; i++)
                            if (sws.Contains<string>(AlphabetTypes[i]))
                            {
                                a = Alphabets[i];
                                break;
                            }
                    

                    bool hasFile = false;
                    bool? longOutput = false;
                    var hasFileType = -1;
                    var s = "";
                    if (sws.Contains<string>("show"))
                        longOutput = true;
                    else if (sws.Contains<string>("hide"))
                        longOutput = null;

                    if (this._conParams.Length > 0)
                        s = this._conParams[0];
                    var p = c.WorkingDirectory + "\\" + s;

                    if (this._conParams != null && this._conParams.Length > 0)
                    {                        
                        if (File.Exists(p))
                        {
                            hasFile = true;
                            for (int i = 0; i < ValidFileTypes.Length; i++)
                                if (ValidFileTypes[i] == s.Substring(s.Length - ValidFileTypes[i].Length, ValidFileTypes[i].Length))
                                {
                                    hasFileType = i;
                                    break;
                                }
                        }
                    }

                    if (hasFileType != -1)
                    {
                        var f = Management.GetTextFromFile(p);
                        List<string> fasta = new List<string>(), seqs = new List<string>(), o = new List<string>();
                        if (ValidFileTypes[hasFileType] == "fasta" || ValidFileTypes[hasFileType] == "txt")
                        {
                            bool startFound = false, endStartFound = false;
                            for (int i = 0; i < f.Length; i++)
                            {
                                //f[i] = f[i].Replace(@", ", "; ");
                                //f[i] = f[i].Replace(@",", "-");

                                if (f[i].Contains(">"))
                                {
                                    endStartFound = false;
                                    fasta.Add(f[i].Trim());
                                    seqs.Add("");
                                    o.Add(f[i].Trim());
                                    startFound = true;

                                    if (true || f[i].Contains("]") || sws.Contains<string>("literal"))
                                        endStartFound = true;

                                    continue;
                                }

                                if (startFound & !endStartFound)
                                {
                                    if (!f[i].Contains("]"))
                                    {
                                        fasta[fasta.Count - 1] += f[i].Trim();
                                        continue;
                                    }
                                    else
                                    {
                                        var ind = f[i].IndexOf("]");
                                        fasta[fasta.Count - 1] += f[i].Substring(0, ind).Trim();
                                        seqs[seqs.Count - 1] += f[i].Substring(ind, f[i].Length - ind).Trim();
                                        endStartFound = true;
                                        continue;
                                    }
                                }

                                if (endStartFound)
                                {
                                    if (!f[i].Contains(">"))
                                    {
                                        seqs[seqs.Count - 1] += f[i].Trim();
                                        continue;
                                    }
                                    else
                                    {
                                        i--;
                                        startFound = false;
                                        continue;
                                    }
                                }

                                                        
                                /*
                                while (i < f.Length && !f[i].Contains(">"))
                                {
                                    var addStr = (string)null;
                                    if (a != null)
                                        addStr = StateSequence.RemoveInvalidAlphas(f[i], a);
                                    else
                                        addStr = f[i];
                                    seqs[seqs.Count - 1] += addStr;
                                    i++;
                                }
                                o.Add(seqs[seqs.Count - 1]);
                                i--;

                                */

                                /*
                                if ((i + 2) % 2 == 0)
                                    fasta.Add(f[i]);

                                else
                                    seqs.Add(f[i]);

                                //outputFeed.Add(f[i]);*/
                            }
                        }
                        else if (ValidFileTypes[hasFileType] == "txt2")
                        {
                            for (int i = 0; i < f.Length; i++)
                            {
                                fasta.Add("");
                                seqs.Add(f[i]);
                                //outputFeed.Add(f[i]);
                            }
                        }

                        this._allFasta = fasta;
                        this._allSeqs = seqs;

                        List<string> t = null;
                        if (a != null)
                        {
                            t = new List<string>();
                            for (int i = 0; i < a.Length; i++)
                                t.Add("" + a[i]);
                        }
                        this._myAlpha.AddMessageSamples(this._allSeqs, t);
                        this._myAlpha.UpdateInfo();

                        var rs = "";
                        if (fasta.Count > 0)
                            rs += fasta.Count + " fasta lines";
                        if (fasta.Count > 0 && seqs.Count > 0)
                            rs += " and ";
                        if (seqs.Count > 0)
                            rs += seqs.Count + " sequence lines";
                        if (rs.Length > 0)
                            rs += " were loaded into object '" + this.FullName + "'.";
                        else
                        {
                            outputFeed.Add("No sequence data was found in the requested file.");
                            return false;
                        }

                        if (longOutput != null && ((!longOutput.Value && o.Count < 350) || longOutput.Value))
                            outputFeed.AddRange(o);
                        outputFeed.AddRange(new string[] { "", rs });
                        return true;
                    }
                    else if (hasFile)
                    {
                        outputFeed.Add("'" + this._conParams[0] + "' is an invalid file type");
                        return false;
                    }
                    
                    outputFeed.Add("Invalid parameters");
                    return false;
                }


                public SeqRef(CommandConsole.ArgClass par, string[] dotHierarchy, string name, string dblprfx, string sfx)
                    : base(par, dotHierarchy, name, dblprfx, sfx)
                { 
                
                
                }
            }

            public static bool? FuncStats(CommandConsole c, Reference existRef, CommandConsole.ReturnArgs retArgs, string[] swtchs, List<string> outputFeed)
            {
                SeqRef sr = null;
                if (existRef.Parent is SeqRef)
                {
                    sr = (SeqRef)existRef.Parent;
                    //outputFeed.AddRange(((SeqRef)existRef.Parent).OutputText);
                }
                
                var a = sr.MyAlphabet;

                var infos = a.MyInfo;

                var allStates = new List<string>();
                for (int i = 0; i < infos.Length; i++)
                {
                    var infoI = infos[i];
                    infoI.UpdateInfo();

                    for (int j = 0; j < infoI.States.Count; j++)
                        allStates.Add(infoI.States[j]);
                }

                var allProbs = new List<List<double>>();
                for (int i = 0; i < infos.Length; i++)
                {
                    allProbs.Add(new List<double>());
                    for (int j = 0; j < allStates.Count; j++)
                    {
                        var queryState = allStates[j];
                                                
                        if (queryState.Length == infos[i].StateLength)
                        {
                            var tind = infos[i].States.IndexOf(queryState);
                            if (tind != -1)
                                allProbs[i].Add(infos[i].StateProbabilities[tind]);
                        }

                    }
                }

                var r = new List<string>();
                var max = 0;
                for (int i = 0; i < allProbs.Count; i++)
                {
                    max = Math.Max(allProbs[i].Count, max);
                }

                for (int i = 0; i < allStates.Count; i++)
                {
                    for (int j = 0; j < infos.Length; j++)
                    {
                        if (r.Count <= i)
                            r.Add(allStates[i]);

                        var curState = allStates[i];
                        var curInfo = infos[j];

                        if (curInfo.StateLength == curState.Length)
                        {
                            var tind = infos[j].States.IndexOf(curState);
                            r[i] += ("\t\t" + curInfo.StateProbabilities[tind].ToString("E5").PadRight(16));
                        }
                        else
                            r[i] += ("\t\t-").PadRight(16);

                        continue;

                    }
                }

                var ts = 0;
                /*
                for (int i = 0; i < allProbs.Count; i++)
                {
                    for (int j = 0; j < max; j++)
                    {
                        if (r.Count <= j)
                            r.Add(allStates[j]);
                        var s = "";

                        if (j >= allProbs[i].Count)
                            s = " - ";
                        else
                        {
                            s = ("" + Math.Round(allProbs[i][j], 4));
                            if (s.Contains('.'))
                                s = s.Substring(1, s.Length - 2);
                        }
                        r[j] += ("\t\t" + s.PadRight(16));
                    }
                }*/


                r.Insert(0, "State");
                for (int i = 0; i < infos.Length; i++)
                    r[0] += "\t\tProbability(n=" + infos[i].StateLength + ")";
                outputFeed.AddRange(r);

                return true;
            }
            public static bool? ToCSV(CommandConsole c, Reference existRef, CommandConsole.ReturnArgs retArgs, string[] swtchs, List<string> outputFeed)
            {
                var sr = (SeqRef)existRef.Parent;
                var n = sr.ArgName;

                if (n == null || n == "")
                {
                    outputFeed.Add("parameter required: file name");
                    return false;
                }

                n += ".csv";

                var ss = new List<string[]>();
                for (int i = 0; i < sr.AllFasta.Count; i++)
                {
                    var s = new string[] { sr.AllFasta[i], sr.AllSeqs[i] };
                    ss.Add(s);
                }

                try
                {
                    Management.WriteTextToCSVFile(c.WorkingDirectory + @"\" + n, ss.ToArray<string[]>(), false);
                }
                catch (Exception e)
                {
                    outputFeed.Add("unable to write csv file.");
                    return false;
                }

                var o = Management.GetTextFromFile(c.WorkingDirectory + @"\" + n);

                return true;
            }

            public StateSequence(char? prfx)
                : base(prfx)
            {
                this._funcs.Add("stats", StateSequence.FuncStats);
                this._funcs.Add("toCSV", StateSequence.ToCSV);
            }

            public override bool? ExecuteCall(CommandConsole c, Reference existRef, CommandConsole.ReturnArgs retArgs, string[] swtchs, List<string> outputFeed)
            {
                return base.ExecuteCall(c, existRef, retArgs, swtchs, outputFeed);
            }

            public override Reference CreateNewReference(CommandConsole.ArgClass par, string[] dotHierarchy, string name, string dblprfx, string sfx)
            {
                return new SeqRef(par, dotHierarchy, name, dblprfx, sfx);
            }
        }


    }
}
