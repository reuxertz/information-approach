using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EvolutionTools
{
    public class LocalBlaster
    {
        public static void PerformLocalBlast(string querypath, string ncbipath, string localpath, string outputpath,
            RunCommand command, BlastType blastType, List<FastaObj> fastaSet, string fastaSetPath, bool inclWaitTime)
        {
            var lb = new LocalBlaster(ncbipath, localpath, outputpath);

            lb.CreateLocalBlastDatabase(command, blastType, fastaSet, fastaSetPath, inclWaitTime);

            lb.PerformBlast(querypath, inclWaitTime);

            //var bo = Program.localBlaster.PerformBlast(queryPath, true);
        }

        public enum BlastType { Protein, Nucleotide, Null };
        public static string[] DBCommands = new string[] { "prot", "nucl" };
        public static string[] BlastCommands = new string[] { "blastp", "blastn" };

        protected string _NCBIBlasteEXEPath, _localDBPath, _outputPath, _fastaSetPath;
        protected string _lastQueryPath;
        protected BlastType _blastType = BlastType.Null;
        protected List<FastaObj> _fastaDBSet;

        protected RunCommand command;

        public List<FastaObj> FastaDBSet
        {
            get
            {
                return this._fastaDBSet;
            }
            set
            {
                this._fastaDBSet = value;
            }
        }

        public LocalBlaster(string ncbiPath, string localDBPath, string outputPath)
        {
            if (Directory.Exists(ncbiPath))
                this._NCBIBlasteEXEPath = ncbiPath;
            if (Directory.Exists(localDBPath))
                this._localDBPath = localDBPath;
            if (Directory.Exists(outputPath))
                this._outputPath = outputPath;

            //Create new Database through c# command line simulator/wrapper. Wait for completion
        }

        public void Reduce(RunCommand command, string tempDirectory, string queryPath, Dictionary<ArCOGObj, List<FastaObj>> dict)
        {
            for (int i = 0; i < dict.Count; i++)
            {
                List<FastaObj> cl;
                dict.TryGetValue(dict.Keys.ToList<ArCOGObj>()[i], out cl);

                this.CreateLocalBlastDatabase(command, LocalBlaster.BlastType.Protein,
                    cl, tempDirectory + @"\tempFastaDB.fa", true);

                int w = 0;
                var cf = new List<FastaObj>(this.FastaDBSet);
                while (w < cf.Count)
                {
                    cf[w].CreateLocalFasta(queryPath);
                    var tbo = this.PerformBlast(queryPath, true);
                    this.RemoveDataBaseRedundancy(tbo, .8, false);
                    w++;
                }

                continue;

            }
        }
        public List<int> RemoveDataBaseRedundancy(BlastObj obj, double removeAboveIdentiyTheshold, Boolean rebuildDatabase)
        {
            var l = new List<int>();

            for (int i = 0; i < obj.entries.Count; i++)
            {
                if (obj.entries[i].blastFasta.giID == obj.queryFasta.giID)
                    continue;

                if (obj.entries[i].Identity > removeAboveIdentiyTheshold)
                {
                    for (int j = 0; j < this._fastaDBSet.Count; j++)
                    {
                        if (this._fastaDBSet[j].giID == obj.entries[i].blastFasta.giID)
                        {
                            l.Add(this._fastaDBSet[j].giID);
                            this._fastaDBSet.RemoveAt(j);
                            break;
                        }
                    }
                }
            }

            if (rebuildDatabase)
                this.CreateLocalBlastDatabase(this.command, this._blastType, this._fastaDBSet, this._fastaSetPath, true);

            return l;
        }

        public void CreateLocalBlastDatabase(RunCommand command, BlastType blastType, List<FastaObj> fastaSet, string fastaSetPath, bool includeWaitTime)
        {
            this._blastType = blastType;
            this._fastaSetPath = fastaSetPath;

            if (this._blastType == BlastType.Null)
                return;

            this.command = command;

            //Name for local db
            if (this._localDBPath.EndsWith(@"\"))
                this._localDBPath += @"\";
            var outFilePath = this._localDBPath + @"\~tempLocalBlastDB";

            //Check for part of db, if exists, remove to force new db creation
            if (File.Exists(outFilePath + ".pin"))
                File.Delete(outFilePath + ".pin");

            var btype = DBCommands[(int)blastType];

            if (fastaSet != null)
            {
                this._fastaDBSet = fastaSet;
                File.Delete(fastaSetPath);
                FastaObj.CreateMultiFastaFile(fastaSetPath, fastaSet);
            }

            var cs = @"makeblastdb -in " + fastaSetPath + " -dbtype " + btype + " -out " + outFilePath;

            if (!File.Exists(fastaSetPath))
                return;

            //Create new Database through c# command line simulator/wrapper. Wait for completion
            command.RunCommandLine(cs, this._NCBIBlasteEXEPath);

            if (includeWaitTime)
                while (command.isRunning)
                { }
        }

        public BlastObj PerformBlast(bool includeWaitTime)
        {
            if (this._blastType == BlastType.Null || this._lastQueryPath == null || !File.Exists(this._lastQueryPath))
                return this.PerformBlast(this._lastQueryPath, includeWaitTime);

            return null;
        }
        public BlastObj PerformBlast(String queryPath, bool includeWaitTime)
        {
            if (this._blastType == BlastType.Null)
                return null;

            this._lastQueryPath = queryPath;

            var cs = LocalBlaster.BlastCommands[(int)this._blastType] +
                    @" -db " + this._localDBPath + @"\~tempLocalBlastDB" +
                    @" -query " + queryPath +
                    @" -out " + _outputPath + @"\~tempBlastOutput.txt";

            this.command.RunCommandLine(cs, this._NCBIBlasteEXEPath); //C:\Ryan\ProteinProj\ProjectFiles\~tempLocalBlast.txt", NCBIExs);

            if (includeWaitTime)
                while (command.isRunning)
                { }

            var bo = new BlastObj(queryPath, _outputPath + @"\~tempBlastOutput.txt");

            /*
            if (this._fastaDBSet != null)
                for (int i = 0; i < bo.entries.Count; i++)
                {
                    for (int j = 0; j < this._fastaDBSet.Count; j++)
                    {
                        if (bo.entries[i].blastFasta.giID == this._fastaDBSet[j].giID)
                        {
                            bo.entries[i].sourceFasta = this._fastaDBSet[j];
                            break;
                        }

                        if (j == this._fastaDBSet.Count - 1)
                            continue;
                    }

                }*/
            return bo;
        }

    }
} 
