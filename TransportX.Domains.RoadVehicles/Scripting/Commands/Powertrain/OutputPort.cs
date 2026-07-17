using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Domains.RoadVehicles.Physics;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Powertrain
{
    public class OutputPort
    {
        public InputPort? ConnectedTo { get; internal set; } = null;
        public float Inertia { get; set; } = 1;

        public Shaft? BuiltShaft { get; private set; } = null;

        public Shaft Build()
        {
            BuiltShaft = new(Inertia);
            return BuiltShaft;
        }
    }
}
