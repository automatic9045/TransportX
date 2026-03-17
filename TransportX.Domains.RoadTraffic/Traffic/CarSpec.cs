using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Domains.RoadTraffic.Traffic
{
    public class CarSpec
    {
        public required float MinThrottle { get; init; }
        public required float MaxThrottle { get; init; }

        public required float MinBrake { get; init; }
        public required float MaxBrake { get; init; }
        public required float EmergencyBrake { get; init; }

        public required float MinBrakeJerk { get; init; }
        public required float MaxBrakeJerk { get; init; }
        public required float EmergencyBrakeJerk { get; init; }

        public required float MaxSpeed { get; init; }
        public required float MaxReverseSpeed { get; init; }

        public required float BrakeLightDecelerationThreshold { get; init; }
    }
}
