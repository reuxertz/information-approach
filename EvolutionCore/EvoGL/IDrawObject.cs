using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionGL
{
    //Draw interface
    public interface IDrawObject
    {
        bool RemoveMe();
        void DrawSelf(EvoGL drawer);
    }
}
