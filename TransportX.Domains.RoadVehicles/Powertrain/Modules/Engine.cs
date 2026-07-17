using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Mathematics;

using TransportX.Domains.RoadVehicles.Physics;

namespace TransportX.Domains.RoadVehicles.Powertrain.Modules
{
    public class Engine : IModule
    {
        public required Curve PerformanceCurve { get; init; }
        public required Curve FrictionCurve { get; init; }

        public Shaft Output { get; }

        public IReadOnlyList<Shaft> InputShafts { get; } = [];
        public IReadOnlyList<Shaft> OutputShafts { get; }
        public IReadOnlyList<IConstraint> Constraints { get; } = [];

        public float AngularVelocity => Output.AngularVelocity;
        public float Rpm => Output.Rpm;

        public float Throttle
        {
            get => field;
            set => field = float.Clamp(value, 0, 1);
        }

        public Engine(Shaft output)
        {
            Output = output;
            OutputShafts = [Output];
        }

        public void Tick(TimeSpan elapsed)
        {
            float friction = FrictionCurve.GetValue(Rpm);

            float maxTorque = PerformanceCurve.GetValue(Rpm) + friction;
            float driveTorque = maxTorque * Throttle;
            Output.ApplyImpulse(driveTorque * (float)elapsed.TotalSeconds);
            float oldAngularVelocity = Output.AngularVelocity;

            float frictionTorque = -float.Sign(Output.AngularVelocity) * friction;
            Output.ApplyImpulse(frictionTorque * (float)elapsed.TotalSeconds);
            if (float.Sign(oldAngularVelocity) != float.Sign(Output.AngularVelocity)) Output.AngularVelocity = 0;

            Output.Torque = driveTorque + frictionTorque;
        }

        public void PropagateTorque()
        {
        }
    }
}
