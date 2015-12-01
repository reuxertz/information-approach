using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using EvolutionTools;

namespace InfoApplication
{
    public class Program
    {
        public static string ProgramTitleName
        {
            get
            {
                return Program.Name + " " + Program.Version;
            }
        }

        public static string Name = "Informatika";
        public static string Version = "1.0";
        public static MainForm MainForm;

        //File Paths
        public static string tempDirectory = @"C:\Ryan\Thesis\temp";
        public static string ncbiDirectory = @"C:\Ryan\Thesis\blast-2.2.30+\bin";
        //public static string arFastasCSVpath = @"C:\Ryan\Thesis\temp\reduced.csv";
        //public static string arFastasCSVpath = @"C:\Ryan\Thesis\temp\original.csv";
        public static string arFastasCSVpath = @"C:\Ryan\Thesis\ArcogData\ar40.csv";
        public static string arCOGCSVpath = @"C:\Ryan\Thesis\ArcogData\arCOG.csv";
        public static string arCOGdefCSVpath = @"C:\Ryan\Thesis\ArcogData\arCOGdef.csv";

        //Constants
        //public static List<string> activeArCOGIDs = ProgramUtil.arCOGsToTest.ToList<string>();//new List<string>() { "arCOG00769" };
        public static List<string> activeArCOGIDs = new List<string>() { "arCOG00769" };
        public static List<string> testArCOGIDs = ProgramUtil.arCOGsToTest.ToList<string>();//new List<string>() { "arCOG00769", "arCOG00069" };// "arCOG01403", "arCOG00130", "arCOG02053" };
        public static List<string> activeFastaIDs = new List<string>() { "18978091" };

        //Raw File Text
        private static string[] arFastasCSV = Management.GetTextFromFile(arFastasCSVpath);
        private static string[] arCOGCSV = Management.GetTextFromFile(arCOGCSVpath);
        private static string[] arCOGdefCSV = Management.GetTextFromFile(arCOGdefCSVpath);

        //Object Storage
        private static List<FastaObj> arFastaObjs = new List<FastaObj>();
        private static List<ArCOGObj> arCOGObjs = new List<ArCOGObj>();

        //Active Storage
        private static List<FastaObj> activeFastas = new List<FastaObj>();
        private static Dictionary<ArCOGObj, List<FastaObj>> activeArCOGFastaClasses = new Dictionary<ArCOGObj, List<FastaObj>>();
        private static List<FastaObj> activeArCOGFastaAll
        {
            get
            {
                var l = new List<FastaObj>();
                var tl = activeArCOGFastaClasses.Values.ToArray<List<FastaObj>>();

                for (int i = 0; i < tl.Length; i++)
                    l.AddRange(tl[i]);

                return l;
            }
        }

        //Objects
        public static RunCommand command = new RunCommand();
        public static LocalBlaster localBlaster = new LocalBlaster(ncbiDirectory, tempDirectory, tempDirectory);

        //Getters / Setters
        public static ArCOGObj GetArCogObjByID(string id)
        {
            foreach (ArCOGObj ac in arCOGObjs)
                if (ac.arCOGID == id)
                    return ac;

            return null;
        }
        public static FastaObj GetFastaObjByID(int id)
        {
            foreach (FastaObj f in arFastaObjs)
                if (f.giID == id)
                    return f;

            return null;
        }
        public static void AddFastaToArcog(FastaObj fasta, ArCOGObj arcog)
        {
            List<FastaObj> fastaList;
            if (!Program.activeArCOGFastaClasses.TryGetValue(arcog, out fastaList))
            {
                fastaList = new List<FastaObj>();
                Program.activeArCOGFastaClasses.Add(arcog, fastaList);
            }

            fastaList.Add(fasta);
        }

        //Data Set Initializers
        public static void InitializeArcogFastas()
        {
            for (int i = 0; i < arFastasCSV.Length; i++)
            {
                var csvSpl = arFastasCSV[i].Split(',');

                if (csvSpl.Length != 2)
                    continue;

                var cont = true;
                for (int j = 0; j < csvSpl[1].Length; j++)
                    if (!ProteinHelper.AminoAcidCharSingleString.Contains(csvSpl[1][j]))
                    {
                        var c = csvSpl[1][j];
                        cont = false;
                        break;
                    }

                if (!cont)
                    continue;

                arFastaObjs.Add(new FastaObj(csvSpl[0], csvSpl[1], i));
            }

            for (int i = 0; i < arCOGdefCSV.Length; i++)
            {
                var csvSpl = arCOGdefCSV[i].Split(',');

                if (csvSpl.Length != 3)
                    continue;

                arCOGObjs.Add(new ArCOGObj(csvSpl[0].Trim(), csvSpl[1], csvSpl[2]));
            }

            //var arCOGs = arCOGCSV.ToList<string>();
            return;
        }
        public static void InitializeArcogFastaClasses()
        {

            var leftOvers = new List<string>();
            for (int i = 0; i < arCOGCSV.Length; i++)
            //while (arCOGs.Count > 0)
            {
                var ca = arCOGCSV[i];

                var curarCOGSpl = ca.Split(',');

                int fastaID = Convert.ToInt32(curarCOGSpl[0].Trim());
                string arCOGID = curarCOGSpl[6].Trim();

                if (!Program.activeArCOGIDs.Contains(arCOGID))
                    continue;

                if (fastaID == -1 || arCOGID == "")
                {
                    leftOvers.Add(ca);
                    //arCOGs.RemoveAt(0);
                    continue;
                }

                var fastaObj = Program.GetFastaObjByID(fastaID);
                var arCOGObj = Program.GetArCogObjByID(arCOGID);

                if (fastaObj == null || arCOGObj == null)
                {
                    leftOvers.Add(ca);
                    //arCOGs.RemoveAt(0);
                    continue;
                }

                fastaObj.arCOG = arCOGObj;
                arCOGObj.fastas.Add(fastaObj);
                

                if (Program.activeFastaIDs.Contains("" + fastaID))
                    Program.activeFastas.Add(fastaObj);

                Program.AddFastaToArcog(fastaObj, arCOGObj);
                //arCOGs.RemoveAt(0);

                if (i % 1000 == 0)
                    System.Diagnostics.Debug.WriteLine(Math.Round(((1.0 * i) / (1.0 * Program.arFastaObjs.Count)) * 100, 2) + "% " + (Program.arFastaObjs.Count - i));
            }

            return;
        }

        public static void Test()
        {
            Program.command.RunCommandLine(@"cd", @"C:\Ryan\Thesis\Informatika_Resources");

        }

        public static void CreateOutput(string path, string[] arCOGsToInclude)
        {
            var txt = new List<string>();
            var cols = new List<string> { "arCOGID", "FastaID" };

            var lst = Program.activeArCOGFastaClasses.Keys.ToList<ArCOGObj>();
            for (int i = 0; i < lst.Count; i++)
            {
                for (int j = 0; j < Program.testArCOGIDs.Count; j++)
                {
                    var arc = lst[i];
                    if (arc.arCOGID == Program.testArCOGIDs[j])
                    {
                        var fastas = new List<FastaObj>();
                        Program.activeArCOGFastaClasses.TryGetValue(arc, out fastas);

                        var y1 = arc.arCOGID;
                        var y3 = new string[400];

                        var ystates = arc.profileCollection.states;
                        for (int l = 0; l < ystates.Count; l++)
                        {
                            var index = cols.IndexOf(ystates[l]) - 2;
                            if (index < 0)
                            {
                                cols.Add(ystates[l]);
                                index = cols.IndexOf(ystates[l]) - 2;
                            }

                            y3[index] = "" + arc.profileCollection.statesProbabilities[l];
                        }


                        var yline = y1 + "," + y1 + ",";
                        for (int l = 0; l < y3.Length; l++)
                        {
                            if (y3[l] == null || y3[l] == "")
                                yline += "0";
                            else
                                yline += y3[l];

                            if (l < y3.Length - 1)
                                yline += ",";
                        }

                        txt.Add(yline);

                        for (int k = 0; k < fastas.Count; k++)
                        {
                            var t1 = arc.arCOGID;
                            var t2 = fastas[k].giID;
                            var t3 = new string[400];

                            var states = fastas[k].entropyProfile.states;
                            for (int l = 0; l < states.Count; l++)
                            {
                                var index = cols.IndexOf(states[l]) - 2;
                                if (index < 0)
                                {
                                    cols.Add(states[l]);
                                    index = cols.IndexOf(states[l]) - 2;
                                }
                                
                                t3[index] = "" + fastas[k].entropyProfile.statesProbabilities[l];
                            }
                            

                            var line = t1 + "," + t2 + ",";
                            for (int l = 0; l < t3.Length; l++)
                            {
                                if (t3[l] == null || t3[l] == "")
                                    line += "0";
                                else
                                    line += t3[l];

                                if (l < t3.Length - 1)
                                    line += ",";
                            }

                            txt.Add(line);

                        }                        
                    }
                }
            }

            var colsStr = "";
            foreach (String s in cols)
                colsStr += s + ",";
            colsStr = colsStr.Remove(colsStr.Length - 2);

            txt.Insert(0, colsStr);

            Management.WriteTextToFile(path, txt, false);
        }
        public static void CreateSequenceProfileOutput(String path)
        {
            var txt = new List<string>();
            var txt2 = new List<string>();
            var cols = new List<string> { "arCOGID", "FastaID", "Species", "Info" };

            var lst = Program.activeArCOGFastaClasses.Keys.ToList<ArCOGObj>();
            for (int i = 0; i < lst.Count; i++)
            {
                for (int j = 0; j < Program.testArCOGIDs.Count; j++)
                {
                    var arc = lst[i];
                    if (arc.arCOGID == Program.testArCOGIDs[j])
                    {
                        var fastas = new List<FastaObj>();
                        Program.activeArCOGFastaClasses.TryGetValue(arc, out fastas);

                        var maxL = 0;
                        for (int k = 0; k < fastas.Count; k++)
                            if (fastas[k].sequence.Length - (fastas[k].entropyProfile.stateFrameWidth - 1) > maxL)
                                maxL = fastas[k].sequence.Length - (fastas[k].entropyProfile.stateFrameWidth - 1);

                        for (int k = 0; k < fastas.Count; k++)
                        {
                            var tline = fastas[k].arCOG.arCOGID + "," + fastas[k].giID + "," + fastas[k].species + "," + fastas[k].info;
                            var tline2 = "" + tline;
                            var tc = 3;

                            var x = fastas[k].getSequenceProfile(arc.profileCollection.states, arc.profileCollection.statesProbabilities,
                                fastas[k].entropyProfile.stateFrameWidth, fastas[k].entropyProfile.stateVelocity);

                            var txt2SeqV = "";
                            var txt2Seq = "" + fastas[k].sequence;
                            for (int l = 0; l < x.Count; l++)
                            {
                                if (!cols.Contains("" + l))
                                {
                                    cols.Add("" + l);
                                }

                                tline += ",\"" + x[l] + "\"";

                                if (x[l] > 1.5)
                                {
                                    txt2SeqV += "*";
                                }
                                else if (x[l] > 1.3)
                                {
                                    txt2SeqV += "+";
                                }
                                else if (x[l] > .8)
                                {
                                    txt2SeqV += " ";
                                }
                                else if (x[l] > .6)
                                {
                                    txt2SeqV += "-";
                                }
                                else
                                {
                                    txt2SeqV += "=";
                                }

                                tc++;
                            }

                            while (tc < maxL)
                            {
                                tline += ",0";
                                tc++;
                            }
                            txt.Add(tline);
                            txt2.Add(tline2 + "," + txt2Seq + "," + txt2SeqV);
                        }
                    }
                }
            }

            var colsStr = "";
            foreach (String s in cols)
                colsStr += s + ",";

            colsStr = colsStr.Remove(colsStr.Length - 2);
            var colsStr2 = "arCOGID,FastaID,Species,Info,sequence,cumulative randomness";

            txt.Insert(0, colsStr);
            txt2.Insert(0, colsStr2);

            Management.WriteTextToFile(path, txt, false);
            Management.WriteTextToFile(path.Replace(".csv", "seqs.csv"), txt2, false);

            return;

        }

        public static void ConvertCSVToDoc(string path)
        {
            var txt = Management.GetTextFromFile(path);
            var r = new List<string>();

            for (int i = 0; i < txt.Length; i++)
            {
                var x = txt[i].Split(',');
                r.AddRange(x);
            }

            Management.WriteTextToFile(path.Replace(".csv", "-2.txt"), r, false);
        }

        static void Main()
        {
            Program.ConvertCSVToDoc(@"C:\Ryan\Thesis\temp\seqoutputseqs.csv");

            Program.InitializeArcogFastas();
            Program.InitializeArcogFastaClasses();

            foreach (ArCOGObj a in Program.activeArCOGFastaClasses.Keys.ToList<ArCOGObj>())
                a.createEntropyProfileCollection(true, 2, 1);

            Program.CreateSequenceProfileOutput(Program.tempDirectory + @"\seqoutput.csv");

            //FastaObj.CreateCSVFromMultiFastaFile(Program.tempDirectory + @"\reduced.fa");
            //FastaObj.CreateCSVFromMultiFastaFile(Program.tempDirectory + @"\original.fa");

            int x = 0;

            //var lb = new LocalBlaster(@"C:\Ryan\ProteinProj\blast-2.2.30+\bin\", );

            //FastaObj.CreateMultiFastaFile(Program.tempDirectory + @"\TEST.txt", arCOGFastaObjLists.Values.ToArray<List<FastaObj>>()[0]);
            /*var t = Management.GetTextFromFile(@"C:\Ryan\Thesis\ArcogData\arCOGIDs.txt");
            var nt = new List<string>();
            var ts = "";
            for (int ii = 0; ii < t.Length; ii++)
            {
                //t[ii] = t[ii].Replace("\"", "").Replace(",", "").Trim();
                t[ii] = "\"" + t[ii] + "\"";

                if (ii < t.Length - 1)
                    t[ii] = t[ii] + ", ";

                ts += " " + t[ii];

                if (ii != 0 && (ii + 1) % 10 == 0)
                {
                    nt.Add(ts);
                    ts = "";
                }
            }*/
            //Management.WriteTextToFile(@"C:\Ryan\Thesis\ArcogData\arCOGIDs.txt", nt.ToArray<string>(), false);

            //Program.localBlaster.CreateLocalBlastDatabase(Program.command, LocalBlaster.BlastType.Protein,
            //    new List<FastaObj>(Program.arCOGFastaAll), tempDirectory + @"\tempFastaDB.fa", true);

            //var queryPath = Program.tempDirectory + @"\tempBlastQuery.txt";
            //Program.activeFastas[0].CreateLocalFasta(queryPath);

            //var bo = Program.localBlaster.PerformBlast(queryPath, true);
            //Program.localBlaster.RemoveDataBaseRedundancy(bo, .8, true);

            /*
            for (int i = 0; i < Program.activeArCOGFastaClasses.Count; i++)
            {
                List<FastaObj> cl;
                Program.activeArCOGFastaClasses.TryGetValue(Program.activeArCOGFastaClasses.Keys.ToList<ArCOGObj>()[i], out cl);

                Program.localBlaster.CreateLocalBlastDatabase(Program.command, LocalBlaster.BlastType.Protein,
                    cl, tempDirectory + @"\tempFastaDB.fa", true);

                int w = 0;
                var cf = new List<FastaObj>(Program.localBlaster.FastaDBSet);
                while (w < cf.Count)
                {
                    cf[w].CreateLocalFasta(queryPath);
                    var tbo = Program.localBlaster.PerformBlast(queryPath, true);                
                    Program.localBlaster.RemoveDataBaseRedundancy(tbo, .8, false);
                    w++;
                }

                continue;

            }/*

            /*int i = 0;
            var l = new List<int>();
            while (i < Program.localBlaster.FastaDBSet.Count)
            {
                Program.localBlaster.FastaDBSet[i].CreateLocalFasta(queryPath);
                var tbo = Program.localBlaster.PerformBlast(queryPath, true);
                
                if (i % 10 == 0)
                l.AddRange(Program.localBlaster.RemoveDataBaseRedundancy(tbo, .8, true));
                i++;
            }*/

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Program.Test();

            Program.MainForm = new MainForm();

            //Application.Run(Program.MainForm);
        }

        public static void Exit()
        {
            Program.MainForm.Close();
        }
    }
}
