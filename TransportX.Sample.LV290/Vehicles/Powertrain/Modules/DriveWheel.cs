using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics.Constraints;

using TransportX.Physics;
using TransportX.Spatial;

using TransportX.Sample.LV290.Vehicles.Powertrain.Physics;

namespace TransportX.Sample.LV290.Vehicles.Powertrain.Modules
{
    internal class DriveWheel : IModule
    {
        private readonly DynamicLocatedModel Wheel;
        private readonly Constraint<AngularAxisMotor> Motor;

        private readonly Shaft Input;
        private readonly int Direction;

        public IEnumerable<IConstraint> Constraints { get; } = [];
        public float AngularVelocity => Input.AngularVelocity;
        public float Velocity => AngularVelocity * 0.959f / 2;
        public float OutTorque => Input.Torque;

        public DriveWheel(DynamicLocatedModel wheel, Constraint<AngularAxisMotor> motor, Shaft input, bool reverseDirection)
        {
            Wheel = wheel;
            Motor = motor;

            Input = input;
            Direction = reverseDirection ? -1 : 1;
        }

        public void Pull()
        {
            Vector3 axis = Vector3.Transform(Direction * Vector3.UnitY, Wheel.Body.Pose.Orientation);
            float bepuVelocity = Vector3.Dot(Wheel.Body.Velocity.Angular, axis);
            Input.AngularVelocity = bepuVelocity;
        }

        public void Push()
        {
            /*Motor.Update(motor =>
            {
                motor.TargetVelocity = float.Sign(Input.Torque) * float.MaxValue;
                motor.Settings.MaximumForce = float.Abs(Input.Torque);
                return motor;
            });*/

            Vector3 axis = Vector3.Transform(Direction * Vector3.UnitY, Wheel.Body.Pose.Orientation);
            float bepuVelocity = Vector3.Dot(Wheel.Body.Velocity.Angular, axis);
            float impulse = (Input.AngularVelocity - bepuVelocity) * Input.Inertia;
            Wheel.Body.ApplyAngularImpulse(axis * impulse);
        }

        public void ApplyTorque(float torque, TimeSpan elapsed)
        {
            Input.ApplyTorque(torque, elapsed);
        }
    }
}
