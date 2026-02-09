using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Rendering
{
    [Flags]
    public enum ModelLoadErrorTypes
    {
        Critical = 1,
        Skipped = 2,
        Visual = 4,
        Collision = 8,
    }
}
