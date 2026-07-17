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
    public class DifferentialFactory : ModuleFactoryBase
    {
        private float FinalRatioValue = 0;

        public InputPort Input { get; } = new();
        public OutputPort OutputL { get; } = new();
        public OutputPort OutputR { get; } = new();

        public new Differential? BuiltModule { get; private set; } = null;

        internal DifferentialFactory(ScriptAvatar avatar, string key) : base(avatar, key)
        {
        }

        public DifferentialFactory FinalRatio(double ratio)
        {
            FinalRatioValue = (float)ratio;
            return this;
        }

        public DifferentialFactory OutputInertia(double inertiaL, double inertiaR)
        {
            OutputL.Inertia = (float)inertiaL;
            OutputR.Inertia = (float)inertiaR;
            return this;
        }

        protected override IModule OnBuild()
        {
            Shaft input = Input.Build();
            Shaft outputL = OutputL.Build();
            Shaft outputR = OutputR.Build();

            BuiltModule = new Differential(input, outputL, outputR, FinalRatioValue);
            return BuiltModule;
        }
    }
}
