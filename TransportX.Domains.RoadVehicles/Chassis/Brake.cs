using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics.Constraints;

using TransportX.Communication;
using TransportX.Mathematics;
using TransportX.Physics;

namespace TransportX.Domains.RoadVehicles.Chassis
{
    public class Brake
    {
        public required Constraint<AngularAxisMotor> Motor { get; init; }
        public required Signal<float>? Signal { get; init; }
        public required Curve TorqueCurve { get; init; }

        public Brake()
        {
        }

        public void Update()
        {
            if (Signal is null) return;

            Motor.Description = Motor.Description with
            {
                Settings = Motor.Description.Settings with
                {
                    MaximumForce = TorqueCurve.GetValue(Signal.Value),
                },
            };
        }
    }
}
