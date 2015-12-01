using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionTools
{
    public struct GeneticValue
    {
        //Private fields
        private double _value;              //Value of GeneValue 
        private double _momentumRetainment;           //Momentum constant of object's Value's change per a mutation
        private double _storedMomentumValue;      //Value of momentum (of change0 stored within genetic value

        //properties
        public double Value
        {
            get
            {
                return this._value;
            }
        }
        public double PosPValue
        {
            get
            {
                var r = this._value;

                if (r < 0)
                    r = r * -1;

                if (r > 1)
                    return r - ((int)r);

                return r;
            }
        }

        //Constructor
        public GeneticValue(double v, double m)
        {
            this._value = v;
            this._momentumRetainment = m;
            this._storedMomentumValue = 0;
        }

        //Mutate
        public void Mutate(double NewValue, double momentumRetainmentOveride, bool adjustValue)
        {
            //Set new momentum value
            this._storedMomentumValue = momentumRetainmentOveride * this._storedMomentumValue + (1 - momentumRetainmentOveride) * NewValue;

            //Add momentum to value
            if (adjustValue)
                this._value += this._storedMomentumValue;
        }
        public void Mutate(double mutationValue, bool adjustValue)
        {
            this.Mutate(mutationValue, this._momentumRetainment, adjustValue);
        }
        public void Drift(bool adjustValue)
        {
            this.Mutate(0, adjustValue);
        }
        //Reproduce
        public GeneticValue Reproduce()
        {
            var ngv = new GeneticValue(this.Value, this._momentumRetainment);

            ngv._storedMomentumValue = this._storedMomentumValue;

            return ngv;
        }
        public static GeneticValue[] Reproduce(GeneticValue[] par)
        {
            var ngv = new GeneticValue[par.Length];

            for (int i = 0; i < ngv.Length; i++)
                ngv[i] = par[i].Reproduce();

            return ngv;
        }
        public static GeneticValue[][] Reproduce(GeneticValue[][] par)
        {
            var ngv = new GeneticValue[par.Length][];

            for (int i = 0; i < par.Length; i++)
                ngv[i] = GeneticValue.Reproduce(par[i]);

            return ngv;
        }
    }
}
