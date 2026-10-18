using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Communication;

namespace TransportX.Domains.RoadVehicles.Lighting
{
    public class BrakeLight
    {
        public Signal<bool> BrakeSignal { get; set; } = new(false);
        public List<ILamp> Lamps { get; } = [];

        public BrakeLight()
        {
        }

        public void Tick(TimeSpan elapsed)
        {
            bool isLighting = BrakeSignal.Value;
            for (int i = 0; i < Lamps.Count; i++)
            {
                Lamps[i].Tick(isLighting, elapsed);
            }
        }
    }
}
