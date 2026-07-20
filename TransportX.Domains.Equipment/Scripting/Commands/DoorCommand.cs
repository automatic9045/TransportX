using System;
using System.Collections.Generic;
using System.Text;

using TransportX.Domains.Equipment.Doors;

namespace TransportX.Domains.Equipment.Scripting.Commands
{
    public class DoorCommand
    {
        public static DoorCommand Empty(string key) => new(key, IDoor.Empty);


        public string Key { get; }
        public IDoor Source { get; }

        public DoorCommand(string key, IDoor source)
        {
            Key = key;
            Source = source;
        }
    }
}
