using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;
using TransportX.Diagnostics;

using TransportX.Scripting;
using TransportX.Scripting.Avatars;
using TransportX.Scripting.Avatars.Commands;
using TransportX.Scripting.Collections;
using TransportX.Scripting.Commands;
using TransportX.Scripting.Worlds;

using TransportX.Domains.Equipment.Doors;

namespace TransportX.Domains.Equipment.Scripting.Commands
{
    public abstract class DoorsBase : IComponentCommand
    {
        internal Signals Signals { get; }
        internal IErrorCollector ErrorCollector { get; }

        public DoorCollectionComponent Source { get; }
        IComponent IComponentCommand.Source => Source;

        private readonly ScriptKeyedList<string, DoorCommand> AllKey;
        public IReadOnlyScriptKeyedList<string, DoorCommand> All => AllKey;

        protected DoorsBase(Signals signals, IErrorCollector errorCollector)
        {
            Signals = signals;
            ErrorCollector = errorCollector;

            Source = new DoorCollectionComponent();

            AllKey = new ScriptKeyedList<string, DoorCommand>(door => door.Key, ErrorCollector, "ドア", DoorCommand.Empty);
        }

        public void Add(DoorCommand door)
        {
            AllKey.Add(door);
            Source.Doors.Add(door.Source);
        }
    }
}
