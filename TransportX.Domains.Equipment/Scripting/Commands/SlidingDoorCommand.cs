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
        public static new SlidingDoorCommand Empty(string key) => new(key, SlidingDoor.Empty());


        public new SlidingDoor Source { get; }

        internal SlidingDoorCommand(string key, SlidingDoor source) : base(key, source)
        {
            Source = source;
        }
    }
}
