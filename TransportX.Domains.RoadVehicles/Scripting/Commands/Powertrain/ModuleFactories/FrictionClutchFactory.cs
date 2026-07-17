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
    public class FrictionClutchFactory : ClutchFactoryBase
    {
        private float FrictionCoefficientValue = 0;

        public new FrictionClutch? BuiltModule { get; private set; } = null;

        internal FrictionClutchFactory(ScriptAvatar avatar, string key) : base(avatar, key)
        {
        }

        public FrictionClutchFactory FrictionCoefficient(double torque)
        {
            FrictionCoefficientValue = (float)torque;
            return this;
        }

        public FrictionClutchFactory OutputInertia(double inertia)
        {
            Output.Inertia = (float)inertia;
            return this;
        }

        protected override ClutchBase OnBuild(Shaft input, Shaft output)
        {
            BuiltModule = new FrictionClutch(input, output)
            {
                FrictionCoefficient = FrictionCoefficientValue,
            };
            return BuiltModule;
        }
    }
}
