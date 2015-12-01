using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionTools
{
    public class Genetics
    {
        //SubClasses
        public class Codon
        {
            //Private Fields
            private Genome _parentGenome;
            private int _parentGenomeIndex;
            private string _codonString;
            private bool _epiSwitch = false;

            //Getters
            public Genome MyGenome
            {
                get
                {
                    return this._parentGenome;
                }
                set
                {

                }
            }

            //Constructor
            public Codon(string cs, bool epi)
            {


            }
        }
        public class Genome
        {
            //Private Fields
            private Genetics _genetics;
            private Codon[] _codons;

            //Constructor
            public Genome(Genetics g, Codon[] codons)
            {
                this._codons = codons;
            }            
        }

        //Private Fields
        private string _alphabet;
        private int _codonLength;
       
        //Constructor
        public Genetics(String alphabet, int codonLength)
        {
            this._alphabet = alphabet;
            this._codonLength = codonLength;
        }

        //Functions
        public Codon GenerateRandomCodon(Random r)
        {
            var x = "";
            while (x.Length < this._codonLength)
                x += this._alphabet.Substring((int)(r.NextDouble() * this._alphabet.Length), 1);

            return new Codon(x, false);
        }

    }
}
