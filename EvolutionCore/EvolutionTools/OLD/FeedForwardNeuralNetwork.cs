using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionTools
{
    public class FeedForwardNeuralNetwork
    {
        //Private Fields
        private double[][] _inputWeights;
        private double[] _outputWeights;

        //Public Fields
        public double[][] InputWeights
        {
            get
            {
                var riw = new double[this._inputWeights.Length][];
                for (int i = 0; i < riw.Length; i++)
                    riw[i] = (double[])this._inputWeights[i].Clone();

                return riw;
            }
        }
        public double[] OutputWeights
        {
            get
            {
                return (double[])this._outputWeights.Clone();
            }
        }

        //Constructor
        public FeedForwardNeuralNetwork(double[] inWeights, double outWeight)
        {
            double[][] nw = new double[1][];
            nw[0] = new double[inWeights.Length];

            for (int i = 0; i < inWeights.Length; i++)
                nw[0][i] = inWeights[i];

            this._inputWeights = nw;

            this._outputWeights = new double[] { outWeight };
        }
        public FeedForwardNeuralNetwork(double[][] inWeights, double[] outWeight)
        {
            double[][] nw = new double[inWeights.Length][];
            for (int i = 0; i < nw.Length; i++)
            {
                nw[i] = new double[inWeights[i].Length];
                for (int j = 0; j < inWeights[i].Length; j++)
                    nw[i][j] = inWeights[i][j];
            }
            this._inputWeights = nw;

            this._outputWeights = new double[outWeight.Length];
            for (int i = 0; i < outWeight.Length; i++)
                this._outputWeights[i] = outWeight[i];
        }

        public static FeedForwardNeuralNetwork Basic(int inputs)
        {
            var wi = new double[inputs];
            for (int i = 0; i < inputs; i++)
                wi[i] = 1.0;

            return new FeedForwardNeuralNetwork(wi, 1.0);
        }

        //Calculate Output
        public static double sharp = 1.737;
        public static double bias = -1.2;
        public static double minOutput = -0.0; // ( <0 )
        public double CreateOutput(double[] inputs)
        {
            //Iterate through each inputweightSet
            var outs = new double[this._inputWeights.Length];
            for (int i = 0; i < this._inputWeights.Length; i++)
            {
                var sum = 0.0;
                var rawinsum = 0.0;
                for (int j = 0; j < this._inputWeights[i].Length; j++)
                {
                    sum += this._inputWeights[i][j] * inputs[j];
                    rawinsum += inputs[j];
                }

                //Perform Sigmoid Function
                var x = 1.0 / (1.0 + Math.Pow(Math.E, -1.0 * (bias + sum) * sharp));
                var o = x * (1.0 + (minOutput * -1.0));
                o = o + minOutput;
                outs[i] = o;// rawinsum;
            }

            //GEt otuput
            var output = 0.0;
            for (int i = 0; i < outs.Length; i++)
                output += outs[i] * this._outputWeights[i];

            return output;
        }
    }
}
