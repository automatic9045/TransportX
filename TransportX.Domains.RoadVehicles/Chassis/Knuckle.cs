using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics.Constraints;

using TransportX.Communication;
using TransportX.Physics;

namespace TransportX.Domains.RoadVehicles.Chassis
{
    public class Knuckle
    {
        private float OldSteeringValue = float.NaN;

        public required Constraint<Hinge> Hinge { get; init; }
        public required Pose BaseToBeam { get; init; }
        public required Signal<float>? SteeringSignal { get; init; }
        public required float SteeringLeftAngle { get; init; }
        public required float SteeringRightAngle { get; init; }

        public Knuckle()
        {
        }

        public void Update()
        {
            if (SteeringSignal is null) return;

            float steeringValue = SteeringSignal.Value;
            if (steeringValue == OldSteeringValue) return;
            OldSteeringValue = steeringValue;

            float angle = (steeringValue < 0 ? SteeringLeftAngle : SteeringRightAngle) * steeringValue;
            Quaternion rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, angle);
            Vector3 axis = Vector3.Transform(Vector3.UnitX, rotation);

            Hinge.Description = Hinge.Description with
            {
                LocalHingeAxisA = Pose.TransformNormal(axis, BaseToBeam),
            };
        }
    }
}
