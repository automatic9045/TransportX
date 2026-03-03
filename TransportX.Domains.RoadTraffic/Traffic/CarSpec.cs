using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Domains.RoadTraffic.Traffic
{
    public class CarSpec
    {
        public required float MinAcceleration { get; init; }
        public required float MaxAcceleration { get; init; }
        public required float MinDeceleration { get; init; }
        public required float MaxDeceleration { get; init; }
        public required float MaxSpeed { get; init; }
        public required float MaxReverseSpeed { get; init; }
    }
}
