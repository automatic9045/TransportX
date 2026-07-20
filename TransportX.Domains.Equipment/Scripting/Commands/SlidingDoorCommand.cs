using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Domains.Equipment.Doors;

namespace TransportX.Domains.Equipment.Scripting.Commands
{
    public class SlidingDoorCommand : DoorCommand
    {
        public static new SlidingDoorCommand Empty(string key) => new(key, SlidingDoor.Empty(key));


        public new SlidingDoor Source { get; }

        public SlidingDoorCommand(string key, SlidingDoor source) : base(key, source)
        {
            Source = source;
        }
    }
}
