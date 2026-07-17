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
    public class EngineFactory : ModuleFactoryBase
    {
        private IReadOnlyCollection<CurvePoint> PerformanceCurvePoints = [new CurvePoint(0, 0)];
        private IReadOnlyCollection<CurvePoint> FrictionCurvePoints = [new CurvePoint(0, 0)];

        public OutputPort Output { get; } = new();

        public new Engine? BuiltModule { get; private set; } = null;

        internal EngineFactory(ScriptAvatar avatar, string key) : base(avatar, key)
        {
        }

        public EngineFactory PerformanceCurve(IReadOnlyCollection<CurvePoint> points)
        {
            PerformanceCurvePoints = points;
            return this;
        }

        public EngineFactory Friction(double torque)
        {
            FrictionCurvePoints = [new CurvePoint(0, (float)torque)];
            return this;
        }

        public EngineFactory FrictionCurve(IReadOnlyCollection<CurvePoint> points)
        {
            FrictionCurvePoints = points;
            return this;
        }

        public EngineFactory OutputInertia(double inertia)
        {
            Output.Inertia = (float)inertia;
            return this;
        }

        protected override IModule OnBuild()
        {
            Shaft output = Output.Build();

            BuiltModule = new Engine(output)
            {
                PerformanceCurve = new Curve(PerformanceCurvePoints),
                FrictionCurve = new Curve(FrictionCurvePoints),
            };
            return BuiltModule;
        }
    }
}
