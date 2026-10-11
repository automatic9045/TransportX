using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Communication;
using TransportX.Components;

namespace TransportX.Domains.RoadVehicles.Lighting
{
    public class LightingComponent : ITickableComponent
    {
        public Blinker Blinker { get; }

        public LightingComponent()
        {
            Blinker = new Blinker();
        }

        public void Tick(TimeSpan elapsed, DateTime now)
        {
            Blinker.Tick(elapsed);
        }
    }
}
