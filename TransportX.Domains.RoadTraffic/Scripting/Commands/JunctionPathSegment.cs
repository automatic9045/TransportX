using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Domains.RoadTraffic.Scripting.Commands
{
    public readonly struct JunctionPathSegment
    {
        public string PathKey { get; }
        public double MinS { get; }
        public double MaxS { get; }

        public JunctionPathSegment(string pathKey, double minS, double maxS)
        {
            PathKey = pathKey;
            MinS = minS;
            MaxS = maxS;
        }

        public static implicit operator JunctionPathSegment(string pathKey) => new(pathKey, 0, double.MaxValue);
        public static implicit operator JunctionPathSegment((string PathKey, double MinS) tuple) => new(tuple.PathKey, tuple.MinS, double.MaxValue);
        public static implicit operator JunctionPathSegment((string PathKey, double MinS, double MaxS) tuple) => new(tuple.PathKey, tuple.MinS, tuple.MaxS);
    }
}
