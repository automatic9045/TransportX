using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Spatial;

using TransportX.Domains.RoadVehicles.Physics;

namespace TransportX.Domains.RoadVehicles.Powertrain.Modules
{
    public class WheelAdapter : IExternalModule
    {
        private readonly DynamicTransformedModel Wheel;
        private readonly int Direction;

        public Shaft Input { get; }

        public IReadOnlyList<Shaft> InputShafts { get; }
        public IReadOnlyList<Shaft> OutputShafts { get; } = [];
        public IReadOnlyList<IConstraint> Constraints { get; } = [];
        
        public WheelAdapter(Shaft input, DynamicTransformedModel wheel, bool reverseDirection)
        {
            Input = input;
            InputShafts = [Input];

            Wheel = wheel;
            Direction = reverseDirection ? -1 : 1;
        }

        public void Tick(TimeSpan elapsed)
        {
        }

        public void PropagateTorque()
        {
        }

        public void Pull()
        {
            Vector3 axis = Vector3.Transform(Direction * Vector3.UnitY, Wheel.Body.Pose.Orientation);
            float bepuVelocity = Vector3.Dot(Wheel.Body.Velocity.Angular, axis);
            Input.AngularVelocity = bepuVelocity;
        }

        public void Push()
        {
            Vector3 axis = Vector3.Transform(Direction * Vector3.UnitY, Wheel.Body.Pose.Orientation);
            float bepuVelocity = Vector3.Dot(Wheel.Body.Velocity.Angular, axis);
            float impulse = (Input.AngularVelocity - bepuVelocity) * Input.Inertia;
            Wheel.Body.ApplyAngularImpulse(axis * impulse);
        }
    }
}
