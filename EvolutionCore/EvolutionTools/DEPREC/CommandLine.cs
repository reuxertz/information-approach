using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace EvolutionTools
{
    //Command Line Classes
    public class CommandConsole : CommandPromptWrapper
    {
        //Classes
        public class ReturnArgs
        {
            public List<object> stack = new List<object>();
        }
        public class ArgClass
        {
            //Ref Class
            public class Reference : ArgClass
            {
                //Ref Fields
                protected string[] _dotHierarchy, _conParams;
                protected string _dblPrefix, _suffix;

                //Properties
                public string[] ConstructionParams
                {
                    get
                    {
                        return this._conParams;
                    }
                }
                public string[] DotHierarchy
                {
                    get
                    {
                        return this._dotHierarchy;
                    }
                }
                public string DoublePrefix
                {
                    get
                    {
                        return this._dblPrefix;
                    }
                }
                public string Suffix
                {
                    get
                    {
                        return this._suffix;
                    }
                }
                public override string FullName
                {
                    get
                    {
                        if (this._argName == null)
                            return base.FullName;

                        var r = "";
                        for (int i = 0; i < this._dotHierarchy.Length; i++)
                        {
                            var s = ".";
                            if (i == 0 || i == this._dotHierarchy.Length - 1)
                                s = "";

                            r += this._dotHierarchy[i] + s;
                        }

                        return r;
                    }
                }
                public override string FullDotName
                {
                    get
                    {
                        if (this._argName == null)
                            return base.FullName;

                        var r = "";
                        for (int i = 0; i < this._dotHierarchy.Length; i++)
                        {
                            var s = ".";
                            if (i == this._dotHierarchy.Length - 1)
                                s = "";

                            r += this._dotHierarchy[i] + s;
                        }

                        return r;
                    }
                }

                //Getters
                public int GetHierarchyIndex(int startIndex, string s)
                {
                    for (int i = startIndex; i < this._dotHierarchy.Length; i++)
                        if (this._dotHierarchy[i].ToLower() == s.ToLower())
                            return i;

                    return -1;
                }

                private Reference()
                { }
                public Reference(ArgClass par, string[] dotHierarchy, string name, string dblprfx, string sfx)
                    : base(par, par._prefix)
                {
                    this._argName = name;
                    this._dotHierarchy = dotHierarchy;
                    this._dblPrefix = dblprfx;
                    this._suffix = sfx;

                    if (sfx != null)
                    {
                        this._conParams = sfx.Substring(1, sfx.Length - 2).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string s in this._conParams)
                            s.Trim();
                    }

                    var k = par._funcs.Keys.ToArray();
                    for (int i = 0; i < k.Length; i++)
                        this._funcs.Add(k[i], par._funcs[k[i]]);

                    return;
                }

                //Functions
                public override bool? ExecuteCall(CommandConsole c, Reference existRef, ReturnArgs retArgs, string[] swtchs, List<string> outputFeed)
                {
                    return false;
                }
                public virtual bool? AttemptConstruct(CommandConsole c, Reference existRef, ReturnArgs retArgs, string[] sws, List<string> outputFeed)
                {
                    return false;
                }
                
                //Command Handler
                public void ExecuteCommand(CommandConsole c, ReturnArgs retArgs, string[] swtchs, List<string> outputFeed)
                {
                    ArgClass existRef = null;
                    if (this._argName != "")
                    {
                    //    existRef = (from ArgClass r in c._argRefs
                    //                where (r is Reference) && r.FullName == this.FullName
                    //                select r).ToList<ArgClass>();
                        existRef = c.GetReference(this);

                    }
                    var existClass = (from ArgClass r in c._argRefs
                                      where !(r is Reference) && r.FullName == "" + this.Prefix
                                      select r).FirstOrDefault();
                    bool hasParenths = this._suffix != null && this._suffix[this._suffix.Length - 1] == ')', stdOut = false, stdEx = false;
                    bool hasFunction = false;
                    if (this.ArgName != null)
                        hasFunction = this.Parent._funcs.ContainsKey(this.ArgName);
                    List<ArgClass> refChain = new List<ArgClass> { this };
                    if (this._dotHierarchy.Length > 1)
                        refChain = existClass.GetReferenceChain(this._dotHierarchy, this._dotHierarchy[this._dotHierarchy.Length - 1]);
                    //ArgClass rf, fnc;
                    //this.GetObjectFunction(this, refChain, out rf, out fnc);
                    
                    //if (hasFunction && (rf == null || fnc == null))
                    //    return;

                    Reference er = null;
                    if (existRef != null && existRef is Reference)
                        er = (Reference)existRef;
                    if (hasParenths)
                    {
                        ////////////////////
                        if (existRef == null && !hasFunction)
                        {
                            bool? cnstrct = this.AttemptConstruct(c, er, retArgs, swtchs, outputFeed);
                            if (cnstrct.HasValue && cnstrct.Value)
                            {
                                if (existRef == null)
                                {
                                    c.AddReference(this);
                                    outputFeed.Add("'" + this.FullName + "' was constructed.");
                                    return;
                                }
                            }
                            else
                            {
                                //if (!cnstrct.HasValue)
                                    outputFeed.Add("'" + this.FullName + "' was unable to be constructed.");
                                return;
                            }
                        }
                        //////////////////////////

                        bool? b = null;
                        if (!hasFunction)
                            b = this.ExecuteCall(c, er, retArgs, swtchs, outputFeed);
                        else
                            b = this._parRef._funcs[this._argName](c, this, retArgs, swtchs, outputFeed);
                        stdEx = b.HasValue && b.Value;

                        if (!stdEx)
                            return;
                    }

                    //Handle no class
                    if (existClass == null)
                        return;

                    if (!stdEx && this._argName == null)
                    {
                        outputFeed.Add("'" + this.FullName + "' is a type.");
                        return;
                    }

                    //Handleno reference
                    if (!stdEx && (existClass == null || existRef == null))
                    {
                        if (!hasFunction)
                            outputFeed.Add("'" + this.FullName + "' is null.");
                        else
                            outputFeed.Add("'" + this.FullName + "' must be called with ().");
                        return;
                    }
                    else if (!stdEx)
                        stdOut = true;

                    //Standard output from object
                    if (stdOut || stdEx)
                    {
                        if (stdOut)
                        {
                            var o = this.StandardOutput;
                            if (o != null && o != "")
                                outputFeed.Add(o);
                            else
                                outputFeed.Add("'" + this.FullName + "' is instantiated.");
                        }
                        if (stdEx)
                            outputFeed.Add("'" + this.FullName + "' was called.");

                        return;
                    }

                    return;
                }
            }

            //Arg enum states
            public static string RefNameChars = "1234567890ABCDEFGHIJLKMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            public static string[] RefDblPrfxs = new string[] { "[]" },
                RefSffxs = new string[] { "()" };

            //Static Getters
            public static string GetDoublePrefix(string dblprfx)
            {
                for (int i = 0; i < RefDblPrfxs.Length; i++)
                    if (dblprfx == RefDblPrfxs[i])
                        return RefDblPrfxs[i];

                return null;
            }
            public static string GetSuffix(string suffix)
            {
                for (int i = 0; i < RefSffxs.Length; i++)
                {
                    if (suffix[0] != RefSffxs[i][0])
                        continue;

                    for (int j = 1; j < suffix.Length; j++)
                        if (suffix[j] == RefSffxs[i][1])
                            return suffix.Substring(0, j + 1);
                }

                return null;
            }
            public static int GetInvalidCharIndex(string s)
            {
                return GetInvalidCharIndex(0, s, false);
            }
            public static int GetInvalidCharIndex(int startIndex, string s, bool returnEnd)
            {
                return GetInvalidCharIndex(startIndex, s, null, returnEnd);
            }
            public static int GetInvalidCharIndex(int startIndex, string s, string ignoreChar, bool returnEnd)
            {
                while (ignoreChar != null && startIndex < s.Length && ignoreChar.Contains(s[startIndex]))
                    startIndex++;

                for (int i = startIndex; i < s.Length; i++)
                    if (!ArgClass.RefNameChars.Contains(s[i]))
                        return i;

                if (returnEnd)
                    return s.Length;
                else
                    return -1;
            }
            public static int GetValidCharIndex(string s)
            {
                for (int i = 0; i < s.Length; i++)
                    if (ArgClass.RefNameChars.Contains(s[i]))
                        return i;

                return -1;
            }

            //Delegates
            public delegate bool? ArgFunction(CommandConsole c, Reference existRef, ReturnArgs retArgs, string[] swtchs, List<string> outputFeed);

            //Fields
            protected char? _prefix = null;
            protected string _argName;
            protected ArgClass _parRef;
            protected List<ArgClass> _subRefs = new List<ArgClass>();
            protected Dictionary<string, ArgFunction> _funcs = new Dictionary<string, ArgFunction>();

            //Setters
            public void AddSubArg(ArgClass r)
            {
                if (!this._subRefs.Contains(r) && (from ArgClass a in this._subRefs
                                                   where a.FullName == r.FullName
                                                   select a).ToList<ArgClass>().Count == 0)
                    this._subRefs.Add(r);
            }
            public void AddFunc(string name, ArgFunction f)
            {
                if (this._funcs.ContainsValue(f) || this._funcs.ContainsKey(name))
                    return;

                this._funcs.Add(name, f);
            }

            //Getters
            public List<ArgClass> GetReferenceChain(string[] dh, string final)
            {
                var r = new List<ArgClass>();
                ArgClass cr = this;
                int index = 0;
                while (cr != null)
                {
                    r.Add(cr);

                    if ((cr._argName == final) ||
                        (cr._argName == null && cr._prefix.HasValue && cr._prefix + "" == final))
                        break;

                    index++;

                    if (index >= dh.Length)
                        break;

                    var ns = dh[index];
                    var i = (from ArgClass la in cr._subRefs
                             where la._argName == ns
                             select la).FirstOrDefault();

                    if (i == null)
                        return null;

                    cr = i;
                    continue;
                }

                if (r.Count == 0)
                    return null;

                return r;
            }

            public void GetObjectFunction(ArgClass caller, List<ArgClass> chain, out ArgClass refArg, out ArgClass funcArg)
            {
                //find final object
                refArg = null;
                funcArg = null;
                int i = 0;
                while (i < chain.Count)
                {
                    if (refArg != null && !(refArg is Reference))
                        break;

                    refArg = chain[i];
                    if (refArg is Reference)
                        i++;
                    else
                        break;
                }

                if (refArg == null)
                    refArg = caller;

                funcArg = refArg;
                while (i < chain.Count)
                {
                    funcArg = chain[i];
                    if (!(funcArg is Reference) || funcArg == null)
                        i++;
                    else
                        break;
                }

            }

            //Properties
            public char? Prefix
            {
                get
                {
                    return this._prefix;
                }
            }
            public string ArgName
            {
                get
                {
                    return this._argName;
                }
            }
            public virtual string FullName
            {
                get
                {
                    return this._prefix + this.ArgName;
                }
            }
            public virtual string FullDotName
            {
                get
                {
                    return this._prefix + this.ArgName;
                }
            }
            public string StandardOutput
            {
                get
                {
                    return null;
                }
            }
            public ArgClass Parent
            {
                get
                {
                    return this._parRef;
                }
            }
            public List<ArgClass> SubRefs
            {
                get
                {
                    return this._subRefs;
                }
            }

            //Constructors
            private ArgClass()
            {
            }
            public ArgClass(char? prfx)
                : this(null, prfx)
            {
            }
            public ArgClass(ArgClass p, char? prfx)
            {
                this._parRef = p;
                this._prefix = prfx;

                //if (prfx.HasValue)
                //    this._argName = "" + prfx.Value;
            }
        
            //SubCreators
            public virtual Reference CreateNewReference(ArgClass par, string[] dotHierarchy, string name, string dblprfx, string sfx)
            {
                return new Reference(par, dotHierarchy, name, dblprfx, sfx);
            }

            //Functions
            public virtual bool? ExecuteCall(CommandConsole c, Reference existRef, ReturnArgs retArgs, string[] swtchs, List<string> outputFeed)
            {
                outputFeed.Add("'" + this.FullName + "' was called.");
                return false;
            }
            public Reference HandleCommand(CommandConsole c, ReturnArgs args, string req, List<string> outputFeed)
            {
                if (req == null || c == null || req.Length == 0)
                    return null;

                //Vars
                string[] dotArgs;
                string argName = null, suffix = null, dblprfx = null;
                int sindent = 0, pindent = 0;

                //Get Prefix
                //var prfxArg = (from LineArgumentHandler la in c._argTypes
                //               where la.Prefix == req[0]
                //               select la).FirstOrDefault();
                if (this._prefix.HasValue && this._prefix.Value != req[0])
                    return null;

                if (this._prefix.HasValue)
                    pindent++;

                //Get Double prefix
                if (req.Length > 2)
                {
                    dblprfx = ArgClass.GetDoublePrefix(req.Substring(pindent, 2));
                    if (dblprfx != null)
                        pindent += 2;
                }

                //Suffix
                if (req.Length >= pindent + 2)
                {
                    int ind1 = req.IndexOf(@"(");

                    if (ind1 != -1)
                    {
                        int ind2 = req.IndexOf(@")", ind1);

                        if (ind1 != -1 && ind2 != -1)
                            suffix = ArgClass.GetSuffix(req.Substring(ind1, (ind2 - ind1) + 1));
                    }
                }
                if (suffix != null)
                    sindent += suffix.Length;

                //Get next invalid char, if same as indented, then
                int invChar = ArgClass.GetInvalidCharIndex(pindent, req, ".", true);
                if (invChar == pindent && invChar < req.Length - sindent)
                    return null;

                //Get Dot Hiearchy and final argument
                string ss = req.Substring(pindent, req.Length - sindent - pindent), ss2 = null;
                if (this._prefix.HasValue)
                    ss2 = this._prefix.Value + "." + ss;
                dotArgs = ss2.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                if (dotArgs.Length > 1)
                    argName = dotArgs[dotArgs.Length - 1];


                if (argName != null)
                {
                    int i = ArgClass.GetInvalidCharIndex(argName);
                    if (i != -1)
                    {
                        outputFeed.Add("'" + argName[i] + "' is an invalid character.");
                        return null;
                    }
                }

                ArgClass par = this;

                
                if (argName != null && argName != "")
                {
                    var ch1 = this.GetReferenceChain(dotArgs, dotArgs[dotArgs.Length - 2]);
                    if (ch1 == null)
                    {
                        outputFeed.Add("'" + ss + "' is null or invalid.");
                        return null;
                    }
                    var ch2 = ch1[ch1.Count - 1];
                    par = ch2;
                }


                //Get boolean states
                //bool refsObj = 

                return this.CreateNewReference(par, dotArgs, argName, dblprfx, suffix);
            }

        }

        //Fields
        protected List<ArgClass> _argRefs = new List<ArgClass>();

        //Getters
        public ArgClass GetReference(ArgClass r)
        {
            string[] chain = r.FullDotName.Split('.');
            var a = (from ArgClass x in this._argRefs
                     where r.FullName == x.FullName
                     select x).ToList<ArgClass>();

            if (a.Count == 1 && chain.Length == 1)
                return a[0];

            if (a.Count > 0 || chain.Length == 0)
                throw new NotImplementedException();

            var cr = this._argRefs;
            ArgClass ca = null;
            var rv = new List<ArgClass>();
            for (int i = 0; i < chain.Length; i++)
            {
                var lca = (from ArgClass x in cr
                           where chain[i] == x.ArgName || chain[i] == "" + x.Prefix
                           select x).ToList<ArgClass>();

                if (lca.Count != 1)
                    return null;

                cr = lca[0].SubRefs;
                ca = lca[0];
            }

            
            return ca;

        }

        //Setters
        public void AddReference(ArgClass o)
        {
            //var l = this._argRefs;

            //Get Static References
            if (o.Parent != null)
            {
                var p = this.GetReference(o.Parent);
                if ((from ArgClass la in o.Parent.SubRefs
                     where la.FullName == o.FullName
                     select la).Count<ArgClass>() > 0 ||
                    this._argRefs.Contains(o))
                    return;

                o.Parent.AddSubArg(o);
                return;                    
            }

            if ((from ArgClass la in this._argRefs
                 where la.FullName == o.FullName
                 select la).Count<ArgClass>() == 0 &&
                 !this._argRefs.Contains(o))
                this._argRefs.Add(o);
        }
        public void AddReferences(ArgClass[] o)
        {
            for (int i = 0; i < o.Length; i++)
                this.AddReference(o[i]);
        }

        //Events
        public new event CommandEvent OnCommand;

        //Constructor
        public CommandConsole(bool init, string initDir, string[] initStrings)
            : this(null, null, init, initDir, initStrings)
        {
        }
        public CommandConsole(string refTypes, ArgClass[] cls, bool init, string initDir, string[] initStrings)
            : base(init, initDir, initStrings)
        {
            for (int i = 0; i < refTypes.Length; i++)
            {
                if (cls[i] == null)
                    this.AddReference(new ArgClass(refTypes[i]));
                else
                    this.AddReference(cls[i]);
            }
        }

        //Functions
        protected override List<string> _HandleCustomCommand(string req)
        {
            var reqs = req.Split(' ');
            var refStack = new List<ArgClass>();
            var r = new List<string>();

            for (int i = 0; i < reqs.Length; i++)
                reqs[i].Trim();

            var retArgs = new ReturnArgs();
            for (int i = 0; i < reqs.Length; i++)
            {
                var s = reqs[i];
                if (s == "")
                    continue;

                //Get Switches
                var sws = new List<string>();
                for (int j = i + 1; j < reqs.Length; j++)
                    if (reqs[j].Length > 0 && reqs[j][0] == '/')
                    {
                        i++;
                        sws.Add(reqs[j].Substring(1, reqs[j].Length - 1));
                    }
                    else
                        break;

                CommandConsole.ArgClass.Reference comRef = null;
                for (int j = 0; j < this._argRefs.Count; j++)
                {
                    comRef = this._argRefs[j].HandleCommand(this, retArgs, s, r);

                    if (comRef == null)
                        continue;

                    break;
                }

                //If no command, run event
                if (comRef == null)
                {
                    var rs = "";
                    for (int j = i; j < reqs.Length; j++)
                    {
                        if (reqs[j] == "")
                            continue;

                        rs += reqs[j];

                        if (j < reqs.Length - 1)
                            rs += " ";
                    }

                    if (this.OnCommand != null)
                        this.OnCommand(this, rs, sws.ToArray<string>(), r);

                    break;
                }

                //Attempt to execulte comref
                comRef.ExecuteCommand(this, retArgs, sws.ToArray<string>(), r);

                continue;
            }

            if (r.Count == 0)
                return null;
            else
                return r;
        }
    }

    public class CommandPromptWrapper
    {
        protected int _lineCount = 0, _linesToSkip = 0, _curLine = 0, _prevReqIndex = 0, _linesPerNext = 1000, _milliUntilDispLoad = 3000;
        protected DateTime? _lastLine = null;
        protected bool _isDone = true, _skip = false, _showErrorLines = false, _promptErrorLines = false;
        protected string _request, _workingDirectory;
        protected List<string> _outs = new List<string>(), _prevWDs = new List<string>(), _prevReqs = new List<string>();
        protected Process _process;

        //Properties
        public bool IsDone
        {
            get
            {
                return this._isDone;
            }
        }
        public bool ShowErrorLines
        {
            get
            {
                return this._showErrorLines;
            }
            set
            {
                this._showErrorLines = value;
            }
        }
        public bool PromptErrorLines
        {
            get
            {
                return this._promptErrorLines;
            }
            set
            {
                this._promptErrorLines = value;
            }
        }
        public List<string> NextLines
        {
            get
            {
                var r = (List<string>)null;
                if (this._outs.Count == 0 || this._curLine > this._outs.Count - 1)
                {
                    var n = DateTime.Now;
                    var w = 0.0;
                    if (this._lastLine != null)
                    {
                        w = n.Subtract(this._lastLine.Value).TotalMilliseconds;
                    }

                    if (w <= this._milliUntilDispLoad)
                    {
                        return null;
                    }
                        
                    r = new List<string> { "Loading..." };
                    this._lastLine = null;

                }

                if (r == null)
                {
                    r = this._outs.GetRange(this._curLine, Math.Min(this._linesPerNext, this._outs.Count - this._curLine));
                    this._lastLine = null;
                }

                this._curLine += r.Count;
                return r;
            }
        }
        public string WorkingDirectory
        {
            get
            {
                return this._workingDirectory;
            }
            set
            {
                if (!Directory.Exists(value))
                    return;

                this._workingDirectory = value;

                if (this.OnWorkingDirectoryChanged != null)
                    this.OnWorkingDirectoryChanged(this, value);

            }
        }

        //Setters
        public void ResetPreviousRequestIndex2()
        {
            this._prevReqIndex = 0;
        }

        //Getters            
        public void GetPreviousRequest(int direction, out string req)//, out string wd)
        {
            var r = this._prevReqs[this._prevReqs.Count - this._prevReqIndex - 1];
            //var w = this._prevWDs[this._prevWDs.Count - this._prevReqIndex - 1];
            this._prevReqIndex++;

            while (this._prevReqIndex < 0)
                this._prevReqIndex += this._prevReqs.Count;
            while (this._prevReqIndex >= this._prevReqs.Count)
                this._prevReqIndex -= this._prevReqs.Count;
           // while (this._prevReqIndex < 0)
           //     this._prevReqIndex += this._prevWDs.Count;
            //while (this._prevReqIndex >= this._prevWDs.Count)
            //    this._prevReqIndex -= this._prevWDs.Count;

            req = r;
            //wd = w;
        }
        public string GetLine(int index)
        {
            if (this._outs.Count == 0 || this._curLine >= this._outs.Count)
                return null;

            return this._outs[index];
        }

        //Events
        public delegate void OutputEvent(CommandPromptWrapper clf);
        public delegate void CommandEvent(CommandPromptWrapper clf, string req, string[] args, List<string> output);
        public delegate void DirectoryChangedEvent(CommandPromptWrapper clf, string newDir);

        public event OutputEvent OnOutput;
        public event CommandEvent OnCommand;
        public event DirectoryChangedEvent OnWorkingDirectoryChanged;

        //Handler
        protected virtual List<string> _HandleCustomCommand(string req)
        {
            var r = new List<string>();
            if (this.OnCommand != null)
                this.OnCommand(this, req, null, r);

            if (r.Count == 0)
                return null;
            else
            r.Add(this.WorkingDirectory + ">");
            return r;
        }
        protected void _HandleCommandLineOutput(DataReceivedEventArgs e, string state)
        {
            string s = e.Data;
            if (state == "Error")
            {
                if (this._promptErrorLines && e.Data != null && e.Data != "")
                    MessageBox.Show(e.Data, "Command Line Error");
                if (!this._showErrorLines)
                    return;
            }

            this._lineCount++;

            if (this._skip && this._lineCount < this._linesToSkip)
                return;

            if (this._lineCount >= this._linesToSkip)
                this._skip = true;

            IEnumerable<string> sa = null;

            if (s == null)
            {
                if (this._request != null && this._request != "")
                {
                    sa = this._HandleCustomCommand(this._request);

                    if (sa == null && this._lineCount == 1)
                    {
                        //this._outs.Add("'" + this._request + "' is not a valid command");
                        //this._outs.Add(this._workingDirectory + ">");
                        sa = new string[] { "'" + this._request + "' is not a valid command." };
                    }

                    if (sa != null)
                    {
                        this._outs.AddRange(sa);
                        this._outs.AddRange(new string[] { " ", this.WorkingDirectory + ">" });
                    }

                    /*if (this._process != null && !this._process.HasExited)// && this._process.)
                    {
                        this._process.Close();
                        this._process.Kill();
                        this._process.Dispose();
                    }*/
                }

                this._isDone = true;
                this._process = null;
            }

            if (s != null)
            {
                this._outs.Add(s);
            }


            if (this.OnOutput != null)
                this.OnOutput(this);

            if (this._isDone)
            {
                this._request = null;
                var o = new List<string>(this._outs);
                var i = o[o.Count - 1].IndexOf('>');

                if (i == -1)
                {
                    var a = 9 * 9;
                    a = a * 7;
                }
                else
                    this.WorkingDirectory = this._outs[this._outs.Count - 1].Substring(0, i);
                return;
            }

            return;
        }

        //Constructor
        public CommandPromptWrapper(bool init, string initDir, string[] initStrings)
        {
            if (initStrings != null)
                for (int i = 0; i < initStrings.Length; i++)
                    this._outs.Add(initStrings[i]);

            if (init)
                this.RunCommandLine("", initDir);

        }

        //Functions
        public void RunCommandLine(string req, string wd)
        {
            this._isDone = false;
            this._request = req;
            this._prevReqs.Add(req);
            this._prevReqIndex = 0;
            this._lineCount = 0;
            this._workingDirectory = wd;

            using (Process p = new Process())
            {
                this._process = p;
                p.StartInfo = new ProcessStartInfo("cmd.exe")
                {
                    //RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    //RedirectStandardError = true,
                    UseShellExecute = false,
                    WorkingDirectory = wd,
                    Arguments = "/K " + req,
                    CreateNoWindow = true
                };
                // event handlers for output & error
                p.OutputDataReceived += (sender, e) => this._HandleCommandLineOutput(e, "Output");
                //p.ErrorDataReceived += (sender, e) => this._HandleCommandLineOutput(e, "Error");

                // start process
                this._lastLine = DateTime.Now;
                p.Start();
                // send command to its input
                //p.StandardInput.Write(req + p.StandardInput.NewLine);
                p.BeginOutputReadLine();
                //p.BeginErrorReadLine();
                //wait
                //this._process.WaitForExit();

                //if (this._process != null)
                //    this._process.Kill();
            }
        }
    }

}
