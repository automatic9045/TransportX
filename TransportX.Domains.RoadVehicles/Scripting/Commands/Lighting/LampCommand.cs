using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Domains.RoadVehicles.Lighting;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Lighting
{
    public class LampCommand
    {
        public static LampCommand Empty(string key) => new(key, ILamp.Empty());


        public string Key { get; }
        public ILamp Lamp { get; }

        internal LampCommand(string key, ILamp lamp)
        {
            Key = key;
            Lamp = lamp;
        }
    }
}
