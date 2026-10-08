using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Domains.Equipment.Doors;

namespace TransportX.Domains.Equipment.Scripting.Commands
{
    public class BifoldDoorCommand : DoorCommand
    {
        public static new BifoldDoorCommand Empty(string key) => new(key, BifoldDoor.Empty());


        public new BifoldDoor Source { get; }

        internal BifoldDoorCommand(string key, BifoldDoor source) : base(key, source)
        {
            Source = source;
        }
    }
}
