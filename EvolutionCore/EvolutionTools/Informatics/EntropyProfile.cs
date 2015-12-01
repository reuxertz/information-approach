using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionTools
{
    public class EntropyProfile
    {
        public List<string> states = new List<string>();
        public List<int> statesCount = new List<int>();
        public List<float> statesProbabilities = new List<float>();
        public int totalStates;
        public int stateFrameWidth, stateVelocity;

        public EntropyProfile(string sequence, string[] acceptableCharacters, int frame, int velocity)
        {
            //this.state = state;
            //this.probability = probability;

            this.stateFrameWidth = frame;
            this.stateVelocity = velocity;

            for (int i = 0; i <= sequence.Length - stateFrameWidth; i += this.stateVelocity)
            {
                var curFrame = sequence.Substring(i, stateFrameWidth);
                var index = states.IndexOf(curFrame);

                totalStates++;

                if (index == -1)
                {
                    states.Add(curFrame);
                    statesCount.Add(1);
                    
                }
                else
                    statesCount[index]++;
            }
            
            this.statesProbabilities = new List<float>();
            for (int i = 0; i < statesCount.Count; i++)
                this.statesProbabilities.Add((1.0f * statesCount[i]) / (1.0f * this.totalStates));
                
        }
    }
}
