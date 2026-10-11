using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Scripting.Collections;

using TransportX.Domains.RoadVehicles.Lighting;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Lighting
{
    public class BlinkerLampGroup : LampGroupBase<BlinkerLampCommand>
    {
        private readonly ICollection<IBlinkerLamp> SourceLamps;

        private readonly ScriptKeyedList<string, BlinkerLampCommand> LampsKey;
        public IReadOnlyScriptKeyedList<string, BlinkerLampCommand> Lamps => LampsKey;

        public BlinkerLampGroup(Lighting parent, ICollection<IBlinkerLamp> sourceLamps, string lampName) : base(parent)
        {
            SourceLamps = sourceLamps;
            LampsKey = new ScriptKeyedList<string, BlinkerLampCommand>(command => command.Key, Parent.Avatar.ErrorCollector, lampName, BlinkerLampCommand.Empty);
        }

        protected override BlinkerLampCommand CreateEmptyLamp(string key) => BlinkerLampCommand.Empty(key);

        public BlinkerLampCommand AddLamp(string key, IBlinkerLamp lamp)
        {
            SourceLamps.Add(lamp);

            BlinkerLampCommand command = new(key, lamp);
            LampsKey.Add(command);

            return command;
        }

        public override BlinkerLampCommand AddLamp(string key, ILamp lamp)
        {
            BlinkerLampAdapter blinkerAdapter = new(lamp);
            return AddLamp(key, blinkerAdapter);
        }
    }
}
