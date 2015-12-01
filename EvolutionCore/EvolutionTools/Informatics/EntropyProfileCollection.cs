using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionTools
{
    public class EntropyProfileCollection
    {
        protected List<EntropyProfile> _profiles = new List<EntropyProfile>();

        public ArCOGObj arCOGObj;
        public List<string> states = new List<string>();
        public List<int> statesCount = new List<int>();
        public List<float> statesProbabilities = new List<float>();
        public int totalStates = 0;

        public void Add(EntropyProfile profile)
        {
            this._profiles.Add(profile);

            for (int i = 0; i < profile.states.Count; i++)
            {
                var index = states.IndexOf(profile.states[i]);

                if (index == -1)
                {
                    this.states.Add(profile.states[i]);
                    this.statesCount.Add(1);
                }
                else
                {
                    this.statesCount[index]++;
                }
            }

            this.totalStates += profile.totalStates;
            this.statesProbabilities = new List<float>();
            for (int i = 0; i < statesCount.Count; i++)
                this.statesProbabilities.Add((1.0f * statesCount[i]) / (1.0f * this.totalStates));
        }

        public EntropyProfileCollection()
        {

        }
    }
}
