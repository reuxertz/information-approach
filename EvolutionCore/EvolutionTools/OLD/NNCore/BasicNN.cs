using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionTools.NNCore
{
    public abstract class BasicNN
    {
        //Fields
        protected Node[] _inputNodes, _outputNodes, _nodes;
        protected Link[] _links;
        protected bool _catchDuplicateWhileIterating = true;

        //Constructor
        protected BasicNN(Node[] inputNodes, Node[] outputNodes, Node[] nodes, Link[] links)
        {
            this._inputNodes = inputNodes;
            this._outputNodes = outputNodes;
            this._nodes = nodes;
            this._links = links;
        }

        //Functions
        public void Iterate(double[] inputs, bool backPropagate)
        {
            //Set Input Nodes
            //Catch
            if (this._inputNodes.Length != inputs.Length)
                throw new Exception("Parameter inputs length is does match inputs of network");

            //Set input values to input parameters
            for (int i = 0; i < inputs.Length; i++)
                this._inputNodes[i].ReceiveInput(inputs[i]);

            //Create Node process Queve
            var nodeQueve = new List<Node>();
            nodeQueve.AddRange(this._inputNodes);

            //Iterate through nodes
            while (nodeQueve.Count != 0)
            {
                //Create current Queve
                var curNodeQueve = nodeQueve;
                nodeQueve = new List<Node>();

                //Process nodeQueve
                for (int i = 0; i < curNodeQueve.Count; i++)
                {
                    var curNode = curNodeQueve[i];

                    var nodesToAdd = curNodeQueve[i].Process();

                    for (int j = 0; j < nodesToAdd.Length; j++)
                        if (!nodeQueve.Contains(nodesToAdd[i]))
                            nodeQueve.Add(nodesToAdd[i]);
                }
            }

            //Reset
            for (int i = 0; i < this._nodes.Length; i++)
            {
                this._nodes[i].IterationIndex--;

                if (this._nodes[i].IterationIndex != 0)
                    throw new Exception("unhandled");
            }   
        }
    }

    public abstract class Link
    {
        //Fields
        protected Node _input, _output;
        protected double _weight;

        //Properties
        public Node InputNode
        {
            get
            {
                return this._input;
            }
        }
        public Node OutputNode
        {
            get
            {
                return this._output;
            }
        }

        //Constructor
        public Link(Node input, Node output, double weight)
        {
            this._input = input;
            this._output = output;
            this._weight = weight;
        }

        //Functions
        public Node ForwardProcess()
        {
            //Check nonnull outpuv value
            if (this._input.OutputValue == null)
                throw new Exception("Unhandled null input");

            this._output.ReceiveInput(this._input.OutputValue.Value * this._weight);
            return this._output;
        }
    }
    public abstract class Node
    {
        //Enum Types
        public enum Type { Input, Output, Hidden }
        public enum ActivationFunction { Flat, Linear, Sigmoid }

        //Public Fields
        public int IterationIndex = 0;

        //Fields
        protected Type _myType = Type.Hidden; 
        protected Link[] _inputLinks, _outputLinks;
        protected double? _outputValue, _holdingValue;
        
        //Activation Fields
        protected ActivationFunction _myFunc = ActivationFunction.Linear;
        protected double _slope = 1, _bias = 0;

        //Properties
        public virtual double? OutputValue
        {
            get
            {
                return this._outputValue;
            }
        }

        //Constructor
        public Node(Type t, ActivationFunction af)
        {
            this._myType = t;
            this._myFunc = af;

            if (t == Type.Input)
                this._myFunc = ActivationFunction.Flat;
        }

        //Private Functions
        private void _PerformActivationFunction()
        {
            if (this._holdingValue == null)
                throw new Exception("unhandled null holdingvalue");

            switch (this._myFunc)
            {
                case ActivationFunction.Flat:
                    this._outputValue = this._holdingValue.Value;
                    break;
                case ActivationFunction.Linear:
                    this._outputValue = this._holdingValue.Value * this._slope - this._bias;
                    break;
                case ActivationFunction.Sigmoid:
                    this._outputValue = 1.0 / (1.0 + Math.Pow(Math.E, (this._holdingValue.Value * this._slope) - this._bias));
                    break;
            }
        }

        //Public Functions
        public void ReceiveInput(double input)
        {
            //If input node Set holding value to input
            if (this._myType == Type.Input)
            {
                //Set output value to input provided
                this._holdingValue = input;
            }

            //Catch unhandled
            throw new Exception("unhandled region");

        }
        public Node[] Process()
        {
            var rList = new List<Node>();

            //If Input process output\            
            //if (this._myType == Type.Input)
            //{
                for (int i = 0; i < this._outputLinks.Length; i++)
                {
                    //Activate
                    this._PerformActivationFunction();

                    //Process Output forward
                    var outputNode = this._outputLinks[i].ForwardProcess();

                    if (rList.Contains(outputNode))
                        throw new Exception("current node links to same node more than once -- unhandled");

                    //Add outputnode to return list
                    rList.Add(outputNode);
                }

                //Return
                this.IterationIndex++;
                return rList.ToArray<Node>();
            //}

            //throw new Exception("Unhandled region");
        }

    }
}
