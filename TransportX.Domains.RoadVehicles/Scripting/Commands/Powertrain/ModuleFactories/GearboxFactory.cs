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
    public class GearboxFactory : ModuleFactoryBase
    {
        private IReadOnlyList<float> GearRatiosValue = [];
        private IReadOnlyList<float> ReverseGearRatiosValue = [];

        public InputPort Input { get; } = new();
        public OutputPort Output { get; } = new();

        public new Gearbox? BuiltModule { get; private set; } = null;

        internal GearboxFactory(ScriptAvatar avatar, string key) : base(avatar, key)
        {
        }

        public GearboxFactory GearRatios(params ReadOnlySpan<double> ratios)
        {
            float[] floatRatios = new float[ratios.Length];
            for (int i = 0; i < ratios.Length; i++) floatRatios[i] = (float)ratios[i];

            GearRatiosValue = floatRatios;
            return this;
        }

        public GearboxFactory ReverseGearRatios(params ReadOnlySpan<double> ratios)
        {
            float[] floatRatios = new float[ratios.Length];
            for (int i = 0; i < ratios.Length; i++) floatRatios[i] = (float)ratios[i];

            ReverseGearRatiosValue = floatRatios;
            return this;
        }

        public GearboxFactory OutputInertia(double inertia)
        {
            Output.Inertia = (float)inertia;
            return this;
        }

        protected override IModule OnBuild()
        {
            Shaft input = Input.Build();
            Shaft output = Output.Build();

            BuiltModule = new Gearbox(input, output)
            {
                GearRatios = GearRatiosValue,
                ReverseGearRatios = ReverseGearRatiosValue,
            };
            return BuiltModule;
        }
    }
}
