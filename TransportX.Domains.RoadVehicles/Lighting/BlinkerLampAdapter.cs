using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Domains.RoadVehicles.Lighting
{
    public class BlinkerLampAdapter : IBlinkerLamp
    {
        public ILamp Lamp { get; }

        public BlinkerLampAdapter(ILamp lamp)
        {
            Lamp = lamp;
        }

        public void Tick(in BlinkerContext context, TimeSpan elapsed)
        {
            Lamp.Tick(context.IsLighting, elapsed);
        }

        public void Reset()
        {
            Lamp.Reset();
        }
    }
}
