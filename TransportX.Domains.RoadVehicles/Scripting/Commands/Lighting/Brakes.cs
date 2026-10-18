using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Communication;

using TransportX.Scripting.Collections;

using TransportX.Domains.RoadVehicles.Lighting;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Lighting
{
    public class Brakes : LampGroupBase<LampCommand>
    {
        private readonly ICollection<ILamp> SourceLamps;

        private readonly ScriptKeyedList<string, LampCommand> LampsKey;
        public IReadOnlyScriptKeyedList<string, LampCommand> Lamps => LampsKey;

        public Brakes(Lighting parent) : base(parent)
        {
            SourceLamps = Parent.Source.BrakeLight.Lamps;
            LampsKey = new ScriptKeyedList<string, LampCommand>(command => command.Key, Parent.Avatar.ErrorCollector, "ブレーキランプ", LampCommand.Empty);
        }

        protected override LampCommand CreateEmptyLamp(string key) => LampCommand.Empty(key);

        public override LampCommand AddLamp(string key, ILamp lamp)
        {
            SourceLamps.Add(lamp);

            LampCommand command = new(key, lamp);
            LampsKey.Add(command);

            return command;
        }

        public Brakes Signal(Signal<bool> signal)
        {
            Parent.Source.BrakeLight.BrakeSignal = signal;
            return this;
        }

        public Brakes Signal(string boolSignalKey)
            => Signal(Parent.Avatar.Commander.Signals.Bool(boolSignalKey));
    }
}
