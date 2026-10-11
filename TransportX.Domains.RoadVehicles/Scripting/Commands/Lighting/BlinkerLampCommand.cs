using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Domains.RoadVehicles.Lighting;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Lighting
{
    public class BlinkerLampCommand
    {
        public static BlinkerLampCommand Empty(string key) => new(key, IBlinkerLamp.Empty());


        public string Key { get; }
        public IBlinkerLamp Lamp { get; }

        internal BlinkerLampCommand(string key, IBlinkerLamp lamp)
        {
            Key = key;
            Lamp = lamp;
        }
    }
}
