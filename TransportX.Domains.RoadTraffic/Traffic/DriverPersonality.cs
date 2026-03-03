using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Domains.RoadTraffic.Traffic
{
    public class DriverPersonality
    {
        public float Factor { get; init; }
        public required float DefaultStopMargin { get; init; }
        public required float TimeHeadway { get; init; }
    }
}
