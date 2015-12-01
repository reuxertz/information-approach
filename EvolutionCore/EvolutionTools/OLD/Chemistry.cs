using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionTools
{
    //Public class 
    public class Chemistry
    {
        //Reaction Class
        public class Reaction
        {
            //Private Fields
            private Matter.ElementType _reactant, _product;
            private bool _reversible = true;
            private double _activationEnergy = 0;
            private int _specificProductUnitRatio;

            //public Properties
            public Matter.ElementType Reactant
            {
                get
                {
                    return this._reactant;
                }
            }
            public Matter.ElementType Product
            {
                get
                {
                    return this._product;
                }
            }
            public bool IsReversible
            {
                get
                {
                    return this._reversible;
                }
            }
           
            //Constructor
            public Reaction(Matter.ElementType reactant, Matter.ElementType product)
            {
                this._reactant = reactant;
                this._product = product;
            }
            public Reaction(Matter.ElementType reactant, Matter.ElementType product, double AE, bool reversible)
                : this(reactant, product)
            {
                this._activationEnergy = AE;
                this._reversible = true;
            }

            //Function
            public bool VerifyReaction(Matter m)
            {
                //Make sure matter is reactant, otherwise set reversed
                var reversed = false;
                if (this._reversible && m.TypeMatches(this._product))
                    reversed = true;

                //If not reversed and m does not equal reactant, throw exception
                if (!m.TypeMatches(this._reactant) && !reversed)
                    throw new Exception("Matter Object provided does not satisfy given conditions");

                //Get Probability of states
                var temp = 1.0 * m.ElementHeat / m.TotalMass;
                if (temp == 0)
                    return false;

                return true;
            }
            public virtual bool React(Matter m)
            {
                //Make sure matter is reactant, otherwise set reversed
                var reversed = false;
                if (this._reversible && m.TypeMatches(this._product))
                    reversed = true;

                //If not reversed and m does not equal reactant, throw exception
                if (!m.TypeMatches(this._reactant) && !reversed)
                    throw new Exception("Matter Object provided does not satisfy given conditions");

                //Get Probability of states
                var temp = 1.0 * m.ElementHeat / m.TotalMass;
                var parts = m.ElementUnits;
                if (temp == 0 || parts == 0)
                    return false;

                //Create Vars
                var muparts = 0;
                var muNparts = 0;
                Matter.ElementType toElement;

                //Handle Case If reversed for muEnergy
                if (reversed)
                {
                    muparts = this._product.Mu;
                    muNparts = this._reactant.Mu;
                    toElement = this._reactant;
                }
                else
                {
                    muparts = this._reactant.Mu;
                    muNparts = this._product.Mu;
                    toElement = this._product;
                }

                var reactKR = Math.Exp((muparts - (muNparts + this._activationEnergy)) / temp);

                if (parts == 0)
                {
                    int a = 7; int b = 3; int c = a / b;
                }

                parts--;
                var ReactionRate = temp * parts * reactKR;

                //If State is greater thanequalto one, then create x amount of new state
                if (ReactionRate > 1)// || StateMuNew >= 1)
                {
                    if (this._reactant.MyName == "DNA")
                    {
                        int a = 7; int b = 3; int c = a / b;
                    }

                    ///////////////////////
                    //Add DATA
                    if (m.Owner != null)
                        m.Owner.MySimulator.MyData.AddData("ReactionRate", ReactionRate);
                    else
                        m.MySimulator.MyData.AddData("ReactionRate", ReactionRate);
                    //////////////////////


                    var enDif = muparts - muNparts;
                    var amt = (int)ReactionRate;

                    if (enDif > 0 && enDif * amt > m.ElementHeat)
                        return false;

                    if (amt > parts)
                        amt = parts;
                    //var amt = (int)StateMuNew;
                    var newChunk = new Matter.MatterChunk(m, toElement, amt);

                    //Handle heat
                    if (enDif < 0)
                        m.ReleaseHeat(amt * Math.Abs(enDif));
                    else if (enDif > 0)
                        m.AbsorbHeat(amt * enDif);
                    else
                        throw new Exception("EnDif is zero or did not interact with environment");
                    var nm = m.AbsorbChunk(newChunk);

                    if (!m.NeedsRemoval && !m.IsRemoved)
                        Matter.HeatNeighborEqualize(nm, m);

                    if (m.IsElementEmpty())
                        m.DegenerateExistence();
                }



                return false;
            }
        }
        public class MolecularReaction : Reaction
        {
            //Molecular Bond Strength
            private int _mBondStrength = 1;

            //Constructor
            public MolecularReaction(Matter.ElementType reactant, Matter.ElementType product, double AE, bool reversible)
                : base(reactant, product, 0, reversible)
            {
                //if (!product.IsMolecule)
                    //throw new Exception("Molecular Properties of Product is not a Molecule");
            }

            //Functions
            //public override bool React(Matter m)
            //{




            //}
        }

        //Static Functions
        public static void EquilibriumFunction(int a, int b, double ap, double bp, double rate, out int aOut, out int bOut)
        {
            //Create Vars
            double alpha = .00001;
            double beta = Convert.ToDouble(ap != 0 && bp != 0);
            double change = ((a * ((beta * bp) + alpha)) - (b * ((beta * ap) + alpha))) /
                ((2 * alpha) + beta * (ap + bp));
            int rdb = (int)(change * rate);

            //Adjust change to outs
            aOut = a - rdb;
            bOut = b + rdb;

            if (aOut + bOut != a + b || double.IsNaN(aOut) || double.IsNaN(bOut))
                throw new Exception("Unhandled output");
        }
        public static void EquilibriumFunction(Matter.Energy e1, Matter.Energy e2, double p1, double p2, double rate)
        {
            int nm1 = -1, nm2 = -1;
            Chemistry.EquilibriumFunction(e1.Value, e2.Value, p1, p2, rate, out nm1, out nm2);
            e1.Value = nm1;
            e2.Value = nm2;
        }
        
        //Private Fields
        private Matter.ElementType[] _typeArray;
        private Reaction[] _reactionArray;

        //Getters
        public Matter.ElementType GetElement(string s)
        {
            foreach (Matter.ElementType met in this._typeArray)
                if (met.MyName == s)
                    return met;

            return null;
        }
        public Matter.ElementType GetElement(Matter.ElementType et)
        {
            foreach (Matter.ElementType met in this._typeArray)
                if (met.MyName == et.MyName)
                    return met;

            return null;
        }

        //Constructor
        public Chemistry(Matter.ElementType[] typeArray, Reaction[] reactArray)
        {
            this._typeArray = typeArray;
            this._reactionArray = reactArray;

            //Link Reactions to appropriate types
            for (int i = 0; i < this._typeArray.Length; i++)
            {
                for (int j = 0; j < this._reactionArray.Length; j++)
                {
                    if (this._typeArray[i].MyName == this._reactionArray[j].Reactant.MyName ||
                        (this._reactionArray[j].IsReversible && this._typeArray[i].MyName == this._reactionArray[j].Product.MyName))
                        this._typeArray[i].AddReaction(this._reactionArray[j]);

                }
            }
        }

    }
    public class Matter : Simulator.IAgent
    {
        //SubClass
        public class ElementType
        {
            //Descriptive Fields
            private string _myName;

            //InteractionFields
            private Chemistry.Reaction[] _reactions = {};
            private bool _permeable = false, _stackable = true;
            private int _massPerUnit, _chemPot = 0, _molPot = 0;

            //Object Propertiese
            public Chemistry.Reaction[] Reactions
            {
                get
                {
                    return this._reactions;
                }
            }

            //Properties
            public bool IsPermeable
            {
                get
                {
                    return this._permeable;
                }
            }
            public bool IsStackable
            {
                get
                {
                    return this._stackable;
                }
            }
            public string MyName
            {
                get
                {
                    return this._myName;
                }
            }
            public int MassPerUnit
            {
                get
                {
                    return this._massPerUnit;
                }
            }
            public int Mu
            {
                get
                {
                    return this._chemPot;
                }
            }
            public int MolMu
            {
                get
                {
                    return this._molPot;
                }
            }

            //Constructor
            public ElementType(String myName, bool permeable, bool stackable, int massPerUnit, int mu)
            {
                this._myName = myName;
                this._permeable = permeable;
                this._stackable = stackable;
                this._massPerUnit = massPerUnit;
                this._chemPot = mu;
            }
            public ElementType(String myName, bool permeable, bool stackable, int mu)
                : this(myName, permeable, stackable, 1, mu)
            {
            }

            //Functions
            public void AddReaction(Chemistry.Reaction cr)
            {
                var rs = this._reactions.ToList<Chemistry.Reaction>();

                if ((from Chemistry.Reaction c in rs
                     where c.Reactant == cr.Reactant && c.Product == cr.Product
                     select c).ToArray().Length > 0)
                    throw new Exception("Reaction Match");

                rs.Add(cr);
                this._reactions = rs.ToArray<Chemistry.Reaction>();
            }
        }
        public class Energy
        {
            private int _value;

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
                    if (this._value == int.MinValue)
                        throw new Exception("caught");

                    this._value = value;
                }
            }

            public Energy()
            {
            }
            public Energy(int val)
            {
                this._value = val;
            }

            public void Absorb(Energy e)
            {
                this._value += e._value;
                e._value = 0;
            }
        }
        public struct MatterChunk
        {
            public ElementType MyType;
            public int ElementUnits;
            public Energy MyHeat, MyCharge;

            public MatterChunk(Matter parMatter, ElementType newType, int elementUnits)
            {
                this.MyType = newType;

                if (parMatter.ElementUnits - elementUnits <= 0)
                {
                    elementUnits = parMatter.ElementUnits;
                    //throw new Exception("unhandled invalid mass");
                    parMatter.NeedsRemoval = true;
                }

                parMatter.ElementUnits -= elementUnits;
                this.ElementUnits = elementUnits;


                var percSplit = (1.0 * elementUnits / (elementUnits + parMatter.ElementUnits));
                this.MyHeat = parMatter.ReleaseHeat((int)(percSplit * parMatter.ElementHeat));
                this.MyCharge = new Energy();
            }
            public MatterChunk(Matter parMatter, int elementUnits)
            {
                this.MyType = parMatter._myType;

                if (parMatter.ElementUnits - elementUnits <= 0)
                {
                    elementUnits = parMatter.ElementUnits;
                    //throw new Exception("This unhandled invalid mass");
                    parMatter.NeedsRemoval = true;
                }

                parMatter.ElementUnits -= elementUnits;
                this.ElementUnits = elementUnits;
                

                this.MyHeat = new Energy();
                this.MyCharge = new Energy();
            }
            public MatterChunk(ElementType type, int elementUnits)
            {
                this.MyType = type;
                this.ElementUnits = elementUnits;
                this.MyHeat = new Energy();
                this.MyCharge = new Energy();
            }
        }

        //Static Functions
        private static double _heatFlowRate = .5;
        private static int _massMergeLimit = 1500;
        private static double _naturalAbsorbtionRation = .05;
        public static void MatterPhyCollision(Matter m1, Matter m2)
        {

            //If not matching but m1 can absorb m2
            if (m2._myType.IsStackable && m1._myType.IsPermeable && m1.TotalMass * _naturalAbsorbtionRation >= m2.TotalMass)
            {
                m1.AbsorbMatter(m2);
                return;
            }
            //Handle complementary case of above
            else if (m1._myType.IsStackable && m2._myType.IsPermeable && m1.TotalMass * _naturalAbsorbtionRation >= m2.TotalMass)
            {
                m2.AbsorbMatter(m1);
                return;
            }
            else if (m1.TotalMass + m2.TotalMass < 1500)
            {
                //If matchign types
                if (m1.TypeMatches(m2))
                {
                    m1.AbsorbMatter(m2);
                    return;
                }

                //If not matching but m1 can absorb m2
                if (m2._myType.IsStackable && m1._myType.IsPermeable)
                {
                    m1.AbsorbMatter(m2);
                    return;
                }

                //Handle complementary case of above
                if (m1._myType.IsStackable && m2._myType.IsPermeable)
                {
                    m2.AbsorbMatter(m1);
                    return;
                }
            }
            else
                Matter.MassNeighborEqualize(m1, m2);

            Matter.HeatNeighborEqualize(m1, m2);
        }
        public static void MassNeighborEqualize(Matter m1, Matter m2)
        {     
            List<Matter> ms1 = new List<Matter>(), ms2 = new List<Matter>();

            //Create side by side list of applicable matter
            if (m1._myType.IsStackable)
                ms1.Add(m1);

            if (m2._myType.IsStackable)
                ms2.Add(m2);

            if (m1._myType.IsPermeable)
            {
                for (int i = 0; i < m1._myMixedContents.Count; i++)
                {
                    if (m1._myMixedContents[i]._myType.IsStackable)
                        ms1.Add(m1._myMixedContents[i]);
                }
            }

            if (m2._myType.IsPermeable)
            {
                for (int i = 0; i < m2._myMixedContents.Count; i++)
                {
                    if (m2._myMixedContents[i]._myType.IsStackable)
                        ms2.Add(m2._myMixedContents[i]);
                }
            }

            //Get Matches for simple equilibrium function and remove from old list
            List<Matter[]> matches = new List<Matter[]>();
            for (int i = 0; i < ms1.Count; i++)
            {
                var x = (from Matter tm2 in ms2
                         where ms1[i].TypeMatches(tm2)
                         select tm2).ToList<Matter>();

                if (x.Count == 0)
                    continue;

                if (x.Count > 1)
                    throw new Exception("Two matches?");

                if (ms1[i] == x[0])
                    throw new Exception("Unknown event by matching objects");
                
                if (Math.Abs(((1.0 * ms1[i].ElementUnits) / x[0].ElementUnits) - 1) > .001)
                    matches.Add(new Matter[] { ms1[i], x[0] });

                ms1.RemoveAt(i);
                i--;
                ms2.Remove(x[0]);
            }

            //FLow what is already matching
            for (int i = 0; i < matches.Count; i++)
            {
                int nmm1 = 0, nmm2 = 0;

                Chemistry.EquilibriumFunction(matches[i][0].ElementUnits, matches[i][1].ElementUnits, 1, 1, 1, out nmm1, out nmm2);

                matches[i][0].ElementUnits = nmm1;
                matches[i][1].ElementUnits = nmm2;
            }

            //Go through non matches
            if (m2._myType.IsPermeable)
            {
                for (int i = 0; i < ms1.Count; i++)
                {
                    int nmm1 = 0, nmm2 = 0;

                    Chemistry.EquilibriumFunction(ms1[i].ElementUnits, 0, 1, 1, 1, out nmm1, out nmm2);

                    if (nmm2 == 0)
                        continue;

                    //ms1[i]._elementMass = nmm1;
                    m2.AbsorbChunk(ms1[i].Split(nmm2));
                }
            }
            if (m1._myType.IsPermeable)
            {
                for (int i = 0; i < ms2.Count; i++)
                {
                    int nmm1 = 0, nmm2 = 0;

                    Chemistry.EquilibriumFunction(ms2[i].ElementUnits, 0, 1, 1, 1, out nmm1, out nmm2);

                    if (nmm2 == 0)
                        continue;

                    //ms2[i]._elementMass = nmm1;
                    m1.AbsorbChunk(ms2[i].Split(nmm2));
                }
            }
        }
        public static void HeatNeighborEqualize(Matter m1, Matter m2)
        {
            Chemistry.EquilibriumFunction(m1._myHeatE, m2._myHeatE, m1.TotalMass, m2.TotalMass, Matter._heatFlowRate);
        }
        public static void InternalHeatEqualize(Matter m)
        {
            if (m._myMixedContents.Count == 0)
                return;

            //Cycle through mixcontents and react heat
            var percentage = 1 / m._myMixedContents.Count;
            foreach (Matter mc in m._myMixedContents)
                Chemistry.EquilibriumFunction(m._myHeatE, mc._myHeatE, m.ElementMass, mc.TotalMass, percentage);          
        }

        //IAgent
        public void SetSimulator(Simulator s)
        {
            this._mySimulator = s;
        }

        //Protected Fields
        protected Simulator _mySimulator;
        //protected Chemistry _myChemistry;
        protected ElementType _myType;

        //Private Fields
        protected Matter _owner;
        private List<Matter> _myMixedContents = new List<Matter>();
        private int _elementUnits, _mixMass;
        private Energy _myHeatE = new Energy(), _myChargeE = new Energy();

        //Properites
        public Simulator MySimulator
        {
            get
            {
                return this._mySimulator;
            }
        }
        public Matter Owner
        {
            get
            {
                return this._owner;
            }
        }
        /*
        public Chemistry MyChemistry
        {
            get
            {
                return this._myChemistry;
            }
        }*/
        public List<Matter> MyMix
        {
            get
            {
                return this._myMixedContents;
            }
        }
        public string ElementTypeName
        {
            get
            {
                return this._myType.MyName;
            }
        }
        public int ElementHeat
        {
            get
            {
                return this._myHeatE.Value;
            }
        }
        public int ElementCharge
        {
            get
            {
                return this._myChargeE.Value;
            }
        }
        public int ElementMass
        {
            get
            {
                return this._elementUnits;
            }
        }
        public int ElementUnits
        {
            get
            {
                return this._elementUnits;
            }
            set
            {
                if (this._owner != null)
                    this._owner.MixMass += (value - this.ElementUnits) * this._myType.MassPerUnit;

                this._elementUnits = value;
            }
        }
        public int TotalMass
        {
            get
            {
                return this.ElementMass + this._mixMass;
            }
        }
        public int MixMass
        {
            get
            {
                return this._mixMass;
            }
            set
            {
                var change = value - this._mixMass;

                if (this._owner != null)
                    this._owner.MixMass += change;

                this._mixMass += change;
            }
        }

        //Getters
        public bool TypeMatches(Matter m)
        {
            if (this._myType.MyName == m._myType.MyName)
                return true;

            return false;
        }
        public bool TypeMatches(MatterChunk m)
        {
            if (this._myType.MyName == m.MyType.MyName)
                return true;

            return false;
        }
        public bool TypeMatches(ElementType t)
        {
            if (this._myType.MyName == t.MyName)
                return true;

            return false;
        }
        public bool TypeMatches(string s)
        {
            if (this._myType.MyName == s)
                return true;

            return false;
        }

        //Constructor
        public Matter(ElementType myType, int elementUnits)
        {
            this._myType = myType;
            this._elementUnits = elementUnits;
        }  
        public Matter(MatterChunk mc)
            : this(mc.MyType, mc.ElementUnits)
        {
            this._myChargeE = mc.MyCharge;
            this._myHeatE = mc.MyHeat;
        }

        //ExistenceFunctions
        public bool _needsRemoval = false, _isRemoved = false;
        public bool NeedsRemoval
        {
            get
            {
                return this._needsRemoval;
            }
            set
            {
                this._needsRemoval = value;
            }
        }
        public bool IsRemoved
        {
            get
            {
                return this._isRemoved;
            }
            set
            {
                this._isRemoved = value;
            }
        }


        public bool IsElementEmpty()
        {
            if (this.ElementUnits > 0)
                return false;

            return true;
        }
        public void DegenerateExistence()
        { 
            if (this.ElementUnits != 0)
            {
                int a = 7 * 8 / 9;
                int b = a * 9;
                int c = a / b * 6;
            }

            if (this._owner == null)
            {
                //Handle Mixture
                var tm = this.TotalMass;
                while (this._myMixedContents.Count > 0)    
                {
                    var cm = this.RemoveMixedContent(this._myMixedContents[0]);
                    var ht = (int)(this._myHeatE.Value * this.ElementMass / tm) + 1;
                    if (ht > cm._myHeatE.Value)
                        ht = cm._myHeatE.Value;
                    cm.AbsorbHeat(ht);
                    this.ReleaseHeat(ht);

                    if (cm.ElementTypeName == "DNA")
                        cm = new Matter(new MatterChunk(cm, cm._myType.Reactions[0].Reactant, cm.ElementUnits));
                       
                    this._mySimulator.AddAgent(cm);   
                }
            
                this._mySimulator.RemoveAgent(this);

                //Handle Heat
                if (this._myHeatE.Value > 0)
                {


                }                    
            }
            else
            {
                //Handle Mixture
                if (this._myMixedContents.Count > 0)
                {
                    int a = 7 * 8 / 9;
                    int b = a * 9;
                    int c = a / b * 6;
                }
                //Remove Energy
                if (this._myHeatE.Value > 0)
                    this._owner._myHeatE.Absorb(this._myHeatE);

                //Remove Physical Traces
                this._owner.RemoveMixedContent(this);
            }
        }

        //Private Functions
        private void _FullTypeAbsorb(MatterChunk m)
        {
            if (this.IsElementEmpty() && this.IsRemoved)
                throw new Exception("Unhandled empty state");

            this.ElementUnits += m.ElementUnits;
            m.ElementUnits = 0;
            this._myHeatE.Absorb(m.MyHeat);
            this._myChargeE.Absorb(m.MyCharge);

            //if (m.MyMixedContents.Count > 0)
            //    throw new Exception("Unhan//ddled mixture contents");

            //if (m._owner != null)
           //     throw new Exception("Unhandled m owner");
        }
        private Matter _AddMixedContent(MatterChunk m)
        {
            if (this.IsElementEmpty() && this.IsRemoved)
                throw new Exception("Unhandled empty state");

            var nm = new Matter(m);
            this._myMixedContents.Add(nm);
            nm._owner = this;
            this.MixMass += nm.TotalMass;
            return nm;
        }
        public Matter RemoveMixedContent(Matter m)
        {
            if (!this._myMixedContents.Contains(m))
                throw new Exception("Mixture does not contain matter input");

            this._myMixedContents.Remove(m);
            this.MixMass -= m.TotalMass;
            m._owner = null;

            return m;
        }

        //Energy Functions
        public void AbsorbHeat(int e)
        {
            if (this.IsElementEmpty() && this.IsRemoved)
                throw new Exception("Unhandled empty state");

            this._myHeatE.Value += e;
        }
        public Energy ReleaseHeat(int e)
        {
            if (this.IsElementEmpty() && this.IsRemoved)
                throw new Exception("Unhandled empty state");

            if (this._myHeatE.Value > e)
            {
                this._myHeatE.Value -= e;
                return new Energy(e);
            }

            e = this._myHeatE.Value;
            this._myHeatE.Value = 0;
            return new Energy(e);
        }
        public void React()
        {
            //Go Through reactions and react
            var rs = this._myType.Reactions;
            for (int i = 0; i < rs.Length; i++)
                rs[i].React(this);
        }

        //Matter Functions
        public virtual MatterChunk Split(int units)
        {
            if (this.ElementUnits == 1)
                throw new Exception("too small");

            return new MatterChunk(this, units);
        }
        public void AbsorbMatter(Matter m)
        {
            var mc = m._myMixedContents.ToArray<Matter>();
            for (int i = 0; i < mc.Length; i++)
                this.AbsorbChunk(new MatterChunk(m.RemoveMixedContent(mc[i]), mc[i].ElementUnits));
            
            var mUnits = m.ElementUnits;
            if (mUnits == 0)
                this.AbsorbHeat(m._myHeatE.Value);
            else
                this.AbsorbChunk(new MatterChunk(m, mUnits));            
            
        }        
        public Matter AbsorbChunk(MatterChunk m)
        {
            //Check for Stackable Exact Match
            if (m.MyType.IsStackable && this.TypeMatches(m))
            {
                this._FullTypeAbsorb(m);
                return this;
            }
            

            //If Permeable, allow entry itno mixed contents
            if (this._myType.IsPermeable)
            {
                for (int i = 0; i < this._myMixedContents.Count; i++)
                {
                    if (m.MyType.IsStackable && this._myMixedContents[i].TypeMatches(m))
                    {
                        this._myMixedContents[i]._FullTypeAbsorb(m);
                        return this._myMixedContents[i];
                    }
                }
                return this._AddMixedContent(m);
            }

            //Send Absorbtion to parent
            if (this._owner != null)
            {
                return this._owner.AbsorbChunk(m);
            }

            //SEnd Absorbtion to environment
            if (this._mySimulator != null)
            {
                var nm = new Matter(m);
                this._mySimulator.AddAgent(nm);
                return nm;
            }

            //Throw uhandled
            throw new Exception("Uhandled region");
        }
        public Matter MixedContentsGet(string typeName)
        {
            foreach (Matter m in this._myMixedContents)
                if (m.TypeMatches(typeName))
                    return m;

            return null;
        }

    }













}
