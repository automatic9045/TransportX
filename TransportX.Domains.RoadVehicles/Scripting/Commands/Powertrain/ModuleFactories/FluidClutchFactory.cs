using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Mathematics;

using TransportX.Scripting.Avatars;

using TransportX.Domains.RoadVehicles.Physics;
using TransportX.Domains.RoadVehicles.Powertrain.Modules;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Powertrain.ModuleFactories
{
    public class FluidClutchFactory : ClutchFactoryBase
    {
        private IReadOnlyCollection<CurvePoint> CapacityFactorCurvePoints = [(0, 0)];
        private IReadOnlyCollection<CurvePoint> TorqueRatioCurvePoints = [(0, 0)];

        private float FrictionCoefficientValue = 0;
        private float FluidCoefficientValue = 0;
        private float CoastFluidCoefficientValue = 0;

        private float LockUpSpeedValue = 1;
        private float ImmediateLockUpSpeedValue = 1;

        public new FluidClutch? BuiltModule { get; private set; } = null;

        internal FluidClutchFactory(ScriptAvatar avatar, string key) : base(avatar, key)
        {
        }

        public FluidClutchFactory CapacityFactorCurve(IReadOnlyCollection<CurvePoint> points)
        {
            CapacityFactorCurvePoints = points;
            return this;
        }

        public FluidClutchFactory TorqueRatioCurve(IReadOnlyCollection<CurvePoint> points)
        {
            TorqueRatioCurvePoints = points;
            return this;
        }

        public FluidClutchFactory Coefficients(double friction, double fluid, double coastFluid)
        {
            FrictionCoefficientValue = (float)friction;
            FluidCoefficientValue = (float)fluid;
            CoastFluidCoefficientValue = (float)coastFluid;
            return this;
        }

        public FluidClutchFactory LockUpSpeed(double normal, double immediate)
        {
            LockUpSpeedValue = (float)normal;
            ImmediateLockUpSpeedValue = (float)immediate;
            return this;
        }

        public FluidClutchFactory OutputInertia(double inertia)
        {
            Output.Inertia = (float)inertia;
            return this;
        }

        protected override ClutchBase OnBuild(Shaft input, Shaft output)
        {
            BuiltModule = new FluidClutch(input, output)
            {
                CapacityFactorCurve = new Curve(CapacityFactorCurvePoints),
                TorqueRatioCurve = new Curve(TorqueRatioCurvePoints),

                FrictionCoefficient = FrictionCoefficientValue,
                FluidCoefficient = FluidCoefficientValue,
                CoastFluidCoefficient = CoastFluidCoefficientValue,

                LockUpSpeed = LockUpSpeedValue,
                ImmediateLockUpSpeed = ImmediateLockUpSpeedValue,
            };
            return BuiltModule;
        }
    }
}
