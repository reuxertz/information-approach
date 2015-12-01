using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionTools
{
    public class NodalNetwork
    {
        public static Random R = new Random();

        public class Link
        {
            //Fields
            private ID _myID;
            private Pointer _inputPointer = null, _outputPointer = null;
            
            //Mutable fields
            private double _naturalWeight = 1;
            private bool _mutatable;

            //Propeties
            public double NaturalWeight
            {
                get
                {
                    return this._naturalWeight;
                }
                set
                {
                    this._naturalWeight = value;
                }
            }

            //Properties
            public int InputIDNum
            {
                get
                {
                    if (this._inputPointer == null)
                        throw new Exception("Unhandled");
                        //return -1;

                    return this._inputPointer.IDNum;
                }
            }
            public int OutputIDNum
            {
                get
                {
                    if (this._outputPointer == null)
                        throw new Exception("Unhandled");
                    //return -1;

                    return this._outputPointer.IDNum;
                }
            }
            public int IDNum
            {
                get
                {
                    return this._myID.IDNum;
                }
            }

            //Construcotr
            public Link(ID id, Pointer ptIn, Pointer ptOut, bool mutatable)
            {
                //Set
                this._myID = id;
                this._inputPointer = ptIn;
                this._outputPointer = ptOut;
                this._mutatable = mutatable;
            }
            public Link(ID id, Pointer ptIn, Pointer ptOut, Link l, bool performDeepCloneMutation)
                : this(id, ptIn, ptOut, false)
            {
                if (!performDeepCloneMutation)
                    throw new Exception("This function does not allow the transfer of object-relative infomration");

                var spread = 3;
                var appeareance = //.1 * 
                    (.25 / 413);
                var change = 0.0;
                
                if (l._mutatable && NodalNetwork.R.NextDouble() <= appeareance)
                    change = (((NodalNetwork.R.NextDouble() - .5) / .5) * spread);

                this._mutatable = l._mutatable;
                this._naturalWeight = l._naturalWeight + change;

            }

            //Functions
            public void Flow()
            {
                var outflow = this._inputPointer.CurrentInfo.Value * this._naturalWeight;

                this._outputPointer.AddCurrentInflow(new Info(outflow));
            }

        }
        public class Pointer
        {
            //enum
            public enum PointerType { Base, Input, Output, Sigmoid }

            //Fields
            private ID _id;
            private PointerType _myType;
            private List<Link> _inputLinks = new List<Link>(), _outputLinks = new List<Link>();
            private Info _currentInfo;
            private List<Info> _currentInflow = new List<Info>();

            //Mutatble fields
            private double _sigScaleIn = 1, _sigOffset = 0, _sigSharp = 1, _sigHeight = 1, _sigBottom = 0;

            //Getters
            public Info CurrentInfo
            {
                get
                {
                    return this._currentInfo;
                }
            }
            public int IDNum
            {
                get
                {
                    return this._id.IDNum;

                }
            }

            //Constructor
            public Pointer(PointerType pt, ID id)
            {
                //Set
                this._id = id;
                this._myType = pt;
            }
            public Pointer(ID id)
                : this(PointerType.Base, id)
            {
            }
            public Pointer(PointerType pt, ID id, Pointer copyNode, bool performDeepCloneMutation)
                : this(pt, id)
            {
                if (!performDeepCloneMutation)
                    throw new Exception("Can only perform deep clone, no object relative information allowed within copy");

                if (pt != PointerType.Sigmoid && pt != PointerType.Output)
                    throw new Exception("Unhandled for non sigmoid pointers");

                var spread = 3;
                var appeareance = //.1 * 
                    (.25 / 8);
                var changes = new List<double>();

                for (int i = 0; i < 5; i++)
                {
                    if (NodalNetwork.R.NextDouble() <= appeareance)
                        changes.Add(((NodalNetwork.R.NextDouble() - .5) / .5) * spread);
                    else
                        changes.Add(0);
                }

                this._sigHeight = copyNode._sigHeight + changes[0];
                this._sigOffset = copyNode._sigOffset + changes[1];
                this._sigSharp = copyNode._sigSharp + changes[2];
                this._sigScaleIn = copyNode._sigScaleIn + changes[3];
                this._sigBottom = copyNode._sigBottom + changes[4];

            }

            //Methods
            public void CompileCurrentInfo()
            {
                //If an input neuron, do not compile any network provided input (only external)
                if (this._myType == PointerType.Input)
                {
                    this._currentInflow.Clear();
                    return;
                }

                //Get input value
                var v = 0.0;
                for (int i = 0; i < this._currentInflow.Count; i++)
                    v += this._currentInflow[i].Value;

                //If a basic neuron, perform sigmoid
                if (this._myType == PointerType.Sigmoid)
                    v = (this._sigHeight - this._sigBottom) / (1 + Math.Pow(Math.E, -1.0 * v// (((v * this._sigScaleIn) + this._sigOffset) * this._sigSharp)
                        )) + this._sigBottom;

                if (this._myType == PointerType.Output)
                    v = (((v * this._sigScaleIn) + this._sigOffset) * this._sigSharp);

                //Set Current output
                this._currentInfo = new Info(v);
                this._currentInflow.Clear();
            }

            //Functions
            public void SetCurrentInfo(Info newInfo)
            {
                this._currentInfo = newInfo;
            }
            public void AddCurrentInflow(Info inflow)
            {
                this._currentInflow.Add(inflow);
            }
            public void SetCurrentInflow(List<Info> newInflow)
            {
                this._currentInflow = newInflow;
            }
            public void ClearInflow()
            {
                this._currentInflow.Clear();
            }
        }
        public struct Info
        {
            //Fields
            private double _value;

            //Properties
            public double Value
            {
                get
                {
                    return this._value;
                }
            }

            public Info(double value)
            {
                this._value = value;
            }
        }

        public enum EnergyCostFunction { Link, Node, NodePlusLink }

        //Fields
        private ID.Generator _PointerIDGen = new ID.Generator(), _LinkIDGen = new ID.Generator();
        private List<Pointer> _myPointers = new List<Pointer>(), _myInputs = new List<Pointer>(), _myOutputs = new List<Pointer>(), _myHidden = new List<Pointer>();
        private List<Link> _myLinks = new List<Link>();
        private List<Info> _myTrainingValues = new List<Info>();
        private EnergyCostFunction _costFunc;

        //Properites
        public int LinkCount
        {
            get
            {
                return this._myLinks.Count;
            }
        }
        public int PointerCount
        {
            get
            {
                return this._myPointers.Count;
            }
        }
        public List<Info> Outputs
        {
            get
            {
                return (from Pointer p in this._myOutputs
                        select p.CurrentInfo).ToList<Info>();  
            }
        }
        public int TotalEnergyCost
        {
            get
            {
                if (this._costFunc == EnergyCostFunction.Link)
                    return this._myLinks.Count;

                if (this._costFunc == EnergyCostFunction.Node)
                    return this._myPointers.Count;

                if (this._costFunc == EnergyCostFunction.NodePlusLink)
                    return this._myLinks.Count + this._myPointers.Count;

                throw new Exception("Unhandled Energy Cost function region. Energy Cost function not defined or null");
            }
        }
        public int RealTimeEnergyCost
        {
            get
            {
                return 1;
            }
        }
        public double Fitness
        {
            get
            {
                if (this._myOutputs.Count != this._myTrainingValues.Count)
                    throw new Exception("Unhandled condition");

                var fitness = 0.0;

                for (int i = 0; i < this._myOutputs.Count; i++)
                {
                    //var t = (this._myTrainingValues[i].Value - this._myOutputs[i].CurrentInfo.Value) / this._myTrainingValues[i].Value;
                    //fitness += 1.0 / Math.Abs(t);

                    var tv = this._myTrainingValues[i].Value;
                    var ov = this._myOutputs[i].CurrentInfo.Value;

                    fitness += ((tv + 1) / (Math.Abs(tv - ov) + 1));
                }

                return fitness;
            }
        }

        //Constructor
        private NodalNetwork()
        {
        }
        public NodalNetwork(int inputs, int outputs)
            : this(inputs, outputs, EnergyCostFunction.Link)
        { }
        public NodalNetwork(int inputs, int outputs, EnergyCostFunction ecf)
        {
            this._costFunc = ecf;
            var hidden = 7;
            var inPtrs = new List<Pointer>();
            var outPtrs = new List<Pointer>();
            var hidPtrs = new List<Pointer>();
            var Links = new List<Link>();

            //Create Inputs
            for (int i = 0; i < inputs; i++)
                inPtrs.Add(new Pointer(Pointer.PointerType.Input, new ID(this._PointerIDGen)));

            //Create Outputs
            for (int i = 0; i < outputs; i++)
                outPtrs.Add(new Pointer(Pointer.PointerType.Output, new ID(this._PointerIDGen)));

            //Create Links from all inputs to all outputs
            //for (int i = 0; hidden == 0 && i < inputs; i++)
            //    for (int o = 0; hidden == 0 && o < outputs; o++)
            //       Links.Add(new Link(new ID(this._LinkIDGen), inPtrs[i], outPtrs[o]));

            //Create Hidden
            for (int i = 0; i < hidden; i++)
                hidPtrs.Add(new Pointer(Pointer.PointerType.Sigmoid, new ID(this._PointerIDGen)));

            //Create Links from all inputs to all hidden
            for (int i = 0; i < inputs; i++)
                for (int h = 0; h < hidden; h++)
                    Links.Add(new Link(new ID(this._LinkIDGen), inPtrs[i], hidPtrs[h], false));

            for (int h = 0; h < hidden; h++)
                for (int o = 0; o < outputs; o++)
                    Links.Add(new Link(new ID(this._LinkIDGen), hidPtrs[h], outPtrs[o], true));

            //Add inptrs and outPtrs to Nodal Netowrk
            var allPtrs = new List<Pointer>(inPtrs);
            allPtrs.AddRange(outPtrs);
            allPtrs.AddRange(hidPtrs);
                        
            this._myInputs = inPtrs;
            this._myOutputs = outPtrs;
            this._myHidden = hidPtrs;
            this._myPointers = allPtrs;
            this._myLinks = Links;
        }

        //Static Constructor
        public NodalNetwork CreateClone()
        {
            var nnn = new NodalNetwork();

            nnn._PointerIDGen = this._PointerIDGen.Clone();
            nnn._LinkIDGen = this._LinkIDGen.Clone();
            nnn._costFunc = this._costFunc;
            var allPtrs = new List<Pointer>();
            var inPtrs = new List<Pointer>();
            var outPtrs = new List<Pointer>();
            var hidPtrs = new List<Pointer>();
            var Links = new List<Link>();

            //Create Inputs
            for (int i = 0; i < this._myInputs.Count; i++)
                inPtrs.Add(new Pointer(Pointer.PointerType.Input, new ID(nnn._PointerIDGen, this._myInputs[i].IDNum)));

            //Create Outputs
            for (int i = 0; i < this._myOutputs.Count; i++)
                outPtrs.Add(new Pointer(Pointer.PointerType.Output, new ID(nnn._PointerIDGen, this._myOutputs[i].IDNum), this._myOutputs[i], true));

            //Create Hidden
            for (int i = 0; i < this._myHidden.Count; i++)
                hidPtrs.Add(new Pointer(Pointer.PointerType.Sigmoid, new ID(nnn._PointerIDGen, this._myHidden[i].IDNum), this._myHidden[i], true));

            //Add inptrs and outPtrs to Nodal Netowrk 
            allPtrs.AddRange(inPtrs);
            allPtrs.AddRange(outPtrs);
            allPtrs.AddRange(hidPtrs);

            nnn._myInputs = inPtrs;
            nnn._myOutputs = outPtrs;
            nnn._myHidden = hidPtrs;
            nnn._myPointers = allPtrs;

            //Clone Links
            for (int i = 0; i < this._myLinks.Count; i++)
            {
                var parLink = this.LinkByID(this._myLinks[i].IDNum);
                Links.Add(new Link(new ID(nnn._LinkIDGen, parLink.IDNum), 
                    nnn.NodeByID(parLink.InputIDNum), nnn.NodeByID(parLink.OutputIDNum), parLink, true));
            }

            //Set Links
            nnn._myLinks = Links;

            return nnn;
        }

        //Functions
        public void AddNode()
        {
            var links = new List<Link>();
            var newPtr = new Pointer(Pointer.PointerType.Sigmoid, new ID(this._PointerIDGen));

            //Input to hidden
            for (int i = 0; i < this._myInputs.Count; i++)
                links.Add(new Link(new ID(this._LinkIDGen), this._myInputs[i], newPtr, false));

            //Hidden to input
            for (int i = 0; i < this._myOutputs.Count; i++)
                links.Add(new Link(new ID(this._LinkIDGen), newPtr, this._myOutputs[i], true));

            this._myLinks.AddRange(links);
            this._myHidden.Add(newPtr);
        }
        public Link LinkByID(int idnum)
        {
            for (int i = 0; i < this._myLinks.Count; i++)
            {
                if (this._myLinks[i].IDNum == idnum)
                    return this._myLinks[i];
            }

            throw new Exception("Unhandled");
        }
        public Pointer NodeByID(int idnum)
        {
            for (int i = 0; i < this._myPointers.Count; i++)
            {
                if (this._myPointers[i].IDNum == idnum)
                    return this._myPointers[i];
            }

            throw new Exception("Unhandled");
        }
        public void ProvideInput(List<double> values)
        {
            if (this._myInputs.Count != values.Count)
                throw new Exception("Nodal Inputs does not match inputs provided. Unhandled exception");

            //////////////////////

            for (int i = 0; i < this._myInputs.Count; i++)
                this._myInputs[i].SetCurrentInfo(new Info(values[i]));
        }
        public void ProvideInput(List<Info> info)
        {
            if (this._myInputs.Count != info.Count)
                throw new Exception("Nodal Inputs does not match inputs provided. Unhandled exception");

            //////////////////////

            for (int i = 0; i < this._myInputs.Count; i++)
                this._myInputs[i].SetCurrentInfo(info[i]);

        }
        public void SetTrainingInfo(List<double> info)
        {
            this._myTrainingValues = (from double i in info
                                      select new Info(i)).ToList<Info>();
        }
        public void SimultaneousIterate()
        {
            for (int iterations = 0; iterations < 2; iterations++)
            {
                for (int i = 0; i < this._myLinks.Count; i++)
                    this._myLinks[i].Flow();

                for (int i = 0; i < this._myPointers.Count; i++)
                    this._myPointers[i].CompileCurrentInfo();
            }
        }


    }
}
