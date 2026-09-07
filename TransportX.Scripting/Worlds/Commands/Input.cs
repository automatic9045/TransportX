using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Input.Configuration;

using TransportX.Scripting.Commands;

namespace TransportX.Scripting.Worlds.Commands
{
    public class Input : InputBase<Input>
    {
        public ScriptWorld World { get; }
        public override InputProfile Profile
        {
            get => World.InputProfile;
            set => World.InputProfile = value;
        }

        public Input(ScriptWorld world, Signals signals) : base(world.InputClient, signals, world.ErrorCollector)
        {
            World = world;
        }

        public override ButtonFactory<Input> AddButton(string key)
        {
            ButtonFactory<Input> buttonFactory = new(this, key);
            return buttonFactory;
        }

        public override AxisFactory<Input> AddAxis(string key, double min, double neutral, double max)
        {
            AxisFactory<Input> axisFactory = new(this, World, key, (float)min, (float)neutral, (float)max);
            return axisFactory;
        }
    }
}
