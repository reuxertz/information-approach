using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionTools
{
    public class ArCOGObj
    {
        public string arCOGID;
        public string letterClass, info;
        public List<FastaObj> fastas = new List<FastaObj>();
        public EntropyProfileCollection profileCollection = new EntropyProfileCollection();

        public ArCOGObj createEntropyProfileCollection(bool updateFastaProfiles, int width, int velocity)
        {
            for (int i = 0; i < fastas.Count; i++)
            {
                if (updateFastaProfiles)
                    fastas[i].updateEntropyProfile(width, velocity);

                profileCollection.Add(fastas[i].entropyProfile);
            }
            this.profileCollection.arCOGObj = this;

            return this;
        }

        public ArCOGObj(string id, string clss, string info)
        {
            this.arCOGID = id;
            this.letterClass = clss;
            this.info = info;
        }

    }
}
