using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Network
{
    public readonly struct LanePathSegmentView
    {
        public LanePathView Path { get; }
        public float MinViewS { get; }
        public float MaxViewS { get; }

        public LanePathSegmentView(LanePathView path, float minViewS, float maxViewS)
        {
            Path = path;
            MinViewS = minViewS;
            MaxViewS = float.Max(minViewS, maxViewS);
        }
    }
}
