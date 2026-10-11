using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Domains.RoadVehicles.Lighting
{
    public readonly struct BlinkerContext
    {
        public required bool IsRequested { get; init; }
        public required float Phase { get; init; }
        public required float DutyCycle { get; init; }

        public bool IsLighting => IsRequested && Phase < DutyCycle;

        public BlinkerContext()
        {
        }
    }
}
