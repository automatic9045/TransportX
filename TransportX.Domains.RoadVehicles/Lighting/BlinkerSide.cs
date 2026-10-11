using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Communication;

namespace TransportX.Domains.RoadVehicles.Lighting
{
    public class BlinkerSide
    {
        public Signal<bool> IsLighting { get; set; } = new(false);
        public List<IBlinkerLamp> Lamps { get; } = [];

        public BlinkerSide()
        {
        }

        public void Tick(in BlinkerContext context, TimeSpan elapsed)
        {
            IsLighting.Value = context.IsLighting;

            for (int i = 0; i < Lamps.Count; i++)
            {
                Lamps[i].Tick(context, elapsed);
            }
        }

        public void Reset()
        {
            IsLighting.Value = false;

            for (int i = 0; i < Lamps.Count; i++)
            {
                Lamps[i].Reset();
            }
        }
    }
}
