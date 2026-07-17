using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;

namespace TransportX.Domains.RoadVehicles.Chassis
{
    public class ChassisComponent : ISubTickableComponent
    {
        public List<Axle> Axles { get; } = [];

        public ChassisComponent()
        {
        }

        public void SubTick(TimeSpan elapsed)
        {
            foreach (Axle axle in Axles)
            {
                axle.Tick(elapsed);
            }
        }
    }
}
