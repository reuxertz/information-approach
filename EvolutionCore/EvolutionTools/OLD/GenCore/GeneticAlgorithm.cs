using System;
using System.Collections.Generic;
using System.Threading;

namespace EvolutionTools
{
    public class GATestClass
    {
        //Private Fields
        private string _name;
        private double[][] _testSets;
        private double _trainingValue;


        //Properties
        public string Name
        {
            get
            {
                return this._name;
            }
        }
        public double[][] TestSets
        {
            get
            {
                var r = new double[this._testSets.Length][];
                for (int i = 0; i < this._testSets.Length; i++)
                {
                    r[i] = new double[this._testSets[i].Length];
                    for (int j = 0; j < this._testSets[i].Length; j++)
                        r[i][j] = 1.0 * this._testSets[i][j];
                }
                return r;
            }
        }
        public double TrainingValue
        {
            get
            {
                return this._trainingValue;
            }
        }

        //Constructor
        public GATestClass(string name, double [][] testSets, double trainingValue)
        {
            this._name = name;
            this._testSets = testSets;
            this._trainingValue = trainingValue;
        }
    }
    public class GATestResults
    {
        //Private Fields
        private Agent _testAgent;
        private double[][] _testSets;
        private double[] _trainingValues;
        private double[] _agentTestOutputs;

        public double[][] TestSets
        {
            get
            {
                var r = new double[this._testSets.Length][];
                for (int i = 0; i < this._testSets.Length; i++)
                    r[i] = (double[])(this._testSets[i].Clone());
                return r;
            }
        }
        public double[] TrainingValue
        {
            get
            {
                return this._trainingValues;
            }
        }
        public double[] AgentTestOutputs
        {
            get
            {
                return this._agentTestOutputs;
            }
        }
        public Agent TestAgent
        {
            get
            {
                return this._testAgent;
            }
        }

        //Constructor
        public GATestResults(Agent testAgent, double[][] testSets, double[] testVals, double[] agentOutputs)
        {
            this._testAgent = testAgent;
            this._testSets = testSets;
            this._trainingValues = testVals;
            this._agentTestOutputs = agentOutputs;
        }
    }

    public abstract class GeneticAlgorithm
    {
        //Private fields
        private object _populationLock = new object();
        private List<Agent> _population;
        private GATestClass[] _testClasses, _activeTests;

        //Properties
        public int PopulationCount
        {
            get
            {
                lock (this._populationLock)
                {
                    return this._population.Count;
                }
            }
        }
        public Agent ZeroAgent()
        {
            return this._population[0];

        }

        //Constructor
        public GeneticAlgorithm(Agent[] basePopulation, GATestClass[] testClasses)
        {
            this._population = new List<Agent>(basePopulation);
            this._testClasses = (GATestClass[])testClasses.Clone();
        }        

        //Functions
        public GATestResults[] PerformPopulationTest(double[][] ts, double[] tvs)
        {
            GATestResults[] nr = new GATestResults[this._population.Count];
            for (int i = 0; i < nr.Length; i++)
            {
                nr[i] = this._population[i].CreateTestResults(ts, tvs);
            }

            return nr;
        }
        public void SetPopulation(List<Agent> newPopulation)
        {
            lock (this._populationLock)
            {
                this._population = new List<Agent>(newPopulation);
            }
        }
    }
    public abstract class Agent
    {
        //Public Functions
        public virtual double CreateOutput(double[] testSet)
        {
            throw new Exception("Abstract");
        }
        public virtual double[] CreateOutputs(double[][] testSets)
        {
            double[] r = new double[testSets.Length];
            var rand = new Random((int)DateTime.Now.Ticks);

            for (int i = 0; i < testSets.Length; i++)
            {
                r[i] = this.CreateOutput(testSets[rand.Next(0, testSets.Length)]);//i]);
            }

            return r;
        }
        public virtual GATestResults CreateTestResults(double[][] testSets, double[] trainVals)
        {
            //Get Std Dev of Outputs
            var outputs = this.CreateOutputs(testSets);
            //for (int i = 0; i < outputs.Length; i++)
            //    outputs[i] = (outputs[i] - trainVals[i]);

            return new GATestResults(this, testSets, trainVals, outputs);
        }
        public abstract Agent GenerateSimpleOffspring();
    }
}
