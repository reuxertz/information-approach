using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionTools
{
    public class Expression
    {
        public delegate int SubExp(double d1, double d2);

        //Order non-dependent
        public static double Add(double d1, double d2)
        {
            return d1 + d2;
        }
        public static double Multiply(double d1, double d2)
        {
            return d1 * d2;
        }

        //Order Dependent  
        //Basic
        public static double Subtract(double d1, double d2)
        {
            return d1 - d2;
        }
        public static double Divide(double d1, double d2)
        {
            return d1 / d2;
        }
        //Exponential
        public static double Power(double d1, double d2)
        {
            return Math.Pow(d1, d2);
        }
        public static double Log(double d1, double d2)
        {
            return Math.Log(d1, d2);
        }
    
        //Fields
        private SubExp _myExpression;

        public Expression(SubExp subExp)
        {
            this._myExpression = subExp;
        }    
    }
}
