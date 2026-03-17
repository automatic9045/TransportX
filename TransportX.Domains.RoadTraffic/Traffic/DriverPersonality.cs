using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Domains.RoadTraffic.Traffic
{
    public class DriverPersonality
    {
        public required float Factor { get; init; }
        public required float TimeHeadway { get; init; }
        public required float StartUpReactionTime { get; init; }
        public required float CreepSpeed { get; init; }
    }
}
