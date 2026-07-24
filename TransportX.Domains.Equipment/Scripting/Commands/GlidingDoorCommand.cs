using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Domains.Equipment.Doors;

namespace TransportX.Domains.Equipment.Scripting.Commands
{
    public class GlidingDoorCommand : DoorCommand
    {
        public static new GlidingDoorCommand Empty(string key) => new(key, GlidingDoor.Empty(key));


        public new GlidingDoor Source { get; }

        public GlidingDoorCommand(string key, GlidingDoor source) : base(key, source)
        {
            Source = source;
        }
    }
}
