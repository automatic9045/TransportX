using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Scripting.Avatars;

using TransportX.Domains.RoadVehicles.Physics;
using TransportX.Domains.RoadVehicles.Powertrain.Modules;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Powertrain.ModuleFactories
{
    public abstract class ClutchFactoryBase : ModuleFactoryBase
    {
        public InputPort Input { get; } = new();
        public OutputPort Output { get; } = new();

        public new ClutchBase? BuiltModule { get; private set; } = null;

        protected ClutchFactoryBase(ScriptAvatar avatar, string key) : base(avatar, key)
        {
        }

        protected override sealed IModule OnBuild()
        {
            Shaft input = Input.Build();
            Shaft output = Output.Build();

            BuiltModule = OnBuild(input, output);
            return BuiltModule;
        }

        protected abstract ClutchBase OnBuild(Shaft input, Shaft output);
    }
}
