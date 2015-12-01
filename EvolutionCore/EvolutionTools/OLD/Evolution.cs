using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionTools
{
    public class Evolution
    {



    }

    public class LifeObject 
    {
        //SubClass
        public class Hive
        {
            //Fields
            private List<LifeObject> _collective = new List<LifeObject>();
            private ID.Generator _myIDGenerator = new ID.Generator();
            private bool _updated = false;
            private object collectiveListLock = new object();
            private List<double> _allPastFitness = new List<double>();
            private double _totalPastFitness = 0;
            private int _totalPastAgents = 0;

            //Properties
            public List<double> AllPastFitness
            {
                get
                {
                    return this._allPastFitness;
                }
            }
            public double TotalPastFitness
            {
                get
                {
                    if (!_updated)
                        this.Update();
                    
                    return this._totalPastFitness;

                    throw new Exception("Unhandled region");
                }
            }
            public int TotalPastAgents
            {
                get
                {
                    if (!_updated)
                        this.Update();

                    return this._totalPastAgents;

                    throw new Exception("Unhandled region");
                }
            }
            public int TotalCurrentAgents
            {
                get
                {
                    var ri = 0;

                    lock (collectiveListLock)
                    {
                        ri = this._collective.Count;
                    }

                    return ri;
                }
            }                
            public bool Updated
            {
                get
                {
                    return this._updated;
                }
                set
                {
                    this._updated = false;
                }
            }   

            //Functions
            public ID NextID()
            {
                return new ID(this._myIDGenerator);
            }
            public void Update()
            {
                if (!_updated)
                {
                    var tf = 0.0;
                    var tn = 0;
                    var apf = new List<double>();
                    lock (collectiveListLock)
                    {
                        tn = this._collective.Count;
                        for (int i = 0; i < tn; i++)
                        {
                            tf += this._collective[i].Fitness;
                            apf.Add(this._collective[i].Fitness);
                        }

                        this._allPastFitness = apf;
                    }
                    this._totalPastFitness = tf;
                    this._totalPastAgents = tn;
                    this._updated = true;
                }
            }
            public void AddLifeObject(LifeObject lo)
            {
                ////
                ////
                ///this has been deactivated at root in Life Object constructor
                ////
                lock (collectiveListLock)
                {
                    this._collective.Add(lo);
                }
            }
            public void RemoveLifeObject(LifeObject lo)
            {
                lock (collectiveListLock)
                {
                    this._collective.Remove(lo);
                }
            }
        }

        //Fields
        public double Temp;
        protected energy2 _EctropyIn, _interalEnergy;
        protected NodalNetwork _myNetwork;
        protected Hive _myHive;
        private bool _removeMe = false, _isRemoved = false;
        private ID _myID;              

        //Properties
        public ID ID
        {
            get
            {
                return this._myID;
            }
        }
        public Hive MyHive
        {
            get
            {
                return this._myHive;
            }
        }
        public NodalNetwork MyNetwork
        {
            get
            {
                return this._myNetwork;
            }
        }
        public double Fitness
        {
            get
            {
                return this._myNetwork.Fitness;
            }
        }
        public int Ectropy
        {
            get
            {
                return this._EctropyIn.Value;
            }
        }
        public bool RemoveMe
        {
            get
            {
                return this._removeMe;
            }
            set
            {
                this._removeMe = value;

                if (value)
                    this._myHive.RemoveLifeObject(this);
            }
        }

        //Constructor
        public LifeObject(Hive h, NodalNetwork nn)
        {
            //Set Energies
            //this._interalEnergy = new Energy(Energy.State.Entropy, nn.EnergyCost);

            //Handle Hive Creation/Setting
            if (h == null)
                this._myHive = new Hive();
            else
                this._myHive = h;

            //Add TO Hive
            //this._myHive.AddLifeObject(this);
            this._myID = this._myHive.NextID();
         
            //Set Neural Netowrk
            this._myNetwork = nn;
        }
        public LifeObject(NodalNetwork nn)
            : this(null, nn)
        {
        } 

        //Functions
        public virtual void PreIterate()
        {
            this._myHive.Updated = false;

            //this.RemoveMe = true;
            //var r = new Random();
            //this.Move(new Point(r.Next(-1, 2), r.Next(-1, 2)));

            //Update Draw            
            //this._myDrawObj.SetCircle(this._myUniversalTileLocation, .5);

            //var inInfo = new List<double>();
            //for (int i = 0; i < 10; i++)
            //{
                //var tr = r.Next(100);
                //inInfo.Add(tr);
            //}

            this._myNetwork.SimultaneousIterate();   
        }
        public virtual void PostIterate()
        {
            //Handle Removal
            //if (this._EctropyIn.Value - this._myNetwork.RealTimeEnergyCost <= 0)
            //{
                //this._removeMe = true;
                //return;
            //}
            //Perofrm maintenance on Network
            //this._EctropyIn.Value -= this._myNetwork.RealTimeEnergyCost;

            //if (this._EctropyIn.Value < 0)
            //    throw new Exception("Dead");
            
        }
        public virtual LifeObject ProduceOffspring()
        {
            //throw new Exception("Unhandled Base Offspring production");

            var nl = new LifeObject(this._myHive, this._myNetwork.CreateClone());

            //this._EctropyIn.Value -= 1;

            return nl;
        }
        public void InputEnergy(energy2 e)
        {
            if (e.MyState == energy2.State.Ectropy)
            {
                this._EctropyIn.Value += e.Export().Value;
                return;
            }

            throw new Exception("Unhandled Energy Input Region");
        }


    }

    public struct energy2
    {
        public enum State { Ectropy, Entropy }

        //Fields
        private State _myState;
        private int _value;
        private bool _closedSystem;

        //Properites
        public State MyState
        {
            get
            {
                return this._myState;
            }
        }
        public int Value
        {
            get
            {
                if (double.IsNaN(this._value) || double.IsInfinity(this._value))
                    throw new Exception("caught");

                return this._value;
            }
            set
            {
                if (this._closedSystem)
                    throw new Exception("Energy system is closed");

                this._value = value;
                if (this._value == int.MinValue)
                    throw new Exception("caught");
            }
        }

        //Constructor
        public energy2(State state)
        {
            this._myState = state;
            this._value = 0;
            this._closedSystem = true;
        }
        public energy2(State state, int value, bool closed)
        {
            this._myState = state;
            this._value = value;
            this._closedSystem = closed;
        }

        //Functions   
        public energy2 Export()
        {
            return this.Export(this.Value);
        }
        public energy2 Export(int value)
        {
            if (this._value == 0)
                throw new Exception("Why request when energy is 0");

            if (this._closedSystem)
                throw new Exception("Unhandled for closed systems");

            if (this.Value < value)
                throw new Exception("Value larger than energy value");

            if (value == 0)
                return new energy2();

            this.Value -= value;

            return new energy2(this._myState, value, this._closedSystem);

        }
        
    }
}
