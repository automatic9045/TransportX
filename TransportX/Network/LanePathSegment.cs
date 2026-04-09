using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Network
{
    public readonly struct LanePathSegment
    {
        public ILanePath Path { get; }
        public float MinS { get; }
        public float MaxS { get; }

        public LanePathSegment(ILanePath path, float minS, float maxS)
        {
            Path = path;
            MinS = minS;
            MaxS = float.Max(minS, maxS);
        }
    }
}
