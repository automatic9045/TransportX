using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Sample.Vehicles.Interfaces;
using Bus.Sample.Vehicles.Powertrain.Physics;

namespace Bus.Sample.Vehicles.Powertrain.Modules
{
    internal class FluidClutch : ClutchBase
    {
        private const float FrictionCoefficient = 900;
        private const float FluidCoefficient = 0.03f;

        private const float LockUpSpeed = 1.5f;
        private const float ImmediateLockUpSpeed = 10f;


        public bool EnableCreep { get; set; } = false;
        public bool Lockup { get; set; } = false;
        public LockupResponseMode LockupMode { get; set; } = LockupResponseMode.Normal;
        public float LockupRate { get; private set; } = 0;

        public FluidClutch(IAxis axis, Shaft input, Shaft output) : base(axis, input, output)
        {
        }

        public override void Tick(TimeSpan elapsed)
        {
            float lockupSpeed = LockupMode switch
            {
                LockupResponseMode.Normal => LockUpSpeed,
                LockupResponseMode.Immediate => ImmediateLockUpSpeed,
                _ => throw new NotSupportedException(),
            };
            LockupRate = float.Clamp(LockupRate + (Lockup ? 1 : -1) * lockupSpeed * (float)elapsed.TotalSeconds, 0, 1);

            if (Engagement < 1e-3f)
            {
                Constraint.IsEnabled = false;
                Output.Torque = 0;
                LockupRate = 0;
                return;
            }

            if (LockupRate == 1)
            {
                if (1 - 1e-3f < Engagement)
                {
                    Constraint.IsEnabled = true;
                }
                else
                {
                    Constraint.IsEnabled = false;

                    float delta = Input.AngularVelocity - Output.AngularVelocity;
                    float torque = float.Sign(delta) * FrictionCoefficient * Engagement;
                    Input.ApplyTorque(-torque, elapsed);
                    Output.ApplyTorque(torque, elapsed);
                }
            }
            else
            {
                Constraint.IsEnabled = false;

                float delta = Input.AngularVelocity - Output.AngularVelocity;

                float fluidTorque = FluidCoefficient * delta * float.Abs(delta);
                float frictionTorque = LockupRate * FrictionCoefficient * float.Sign(delta);

                float maxStabilizingTorque = float.Abs(delta) * float.Min(Input.Inertia, Output.Inertia) / (float)elapsed.TotalSeconds;
                float transmittedTorque = float.Clamp(fluidTorque + frictionTorque, -maxStabilizingTorque, maxStabilizingTorque) * Engagement;

                float torqueMultiplier = 1;
                if (EnableCreep && 1 < Input.AngularVelocity && 0 <= Output.AngularVelocity)
                {
                    float speedRatio = float.Max(0, Output.AngularVelocity / Input.AngularVelocity);
                    float speedFactor = float.Min(float.Lerp(1, 10, speedRatio * 10), float.Clamp(float.Lerp(1000, 1, speedRatio / 0.85f), 1, 5));
                    float rpmFactor = float.Clamp(1 - (Input.Rpm - 650) / (700 - 650), 0, 1);
                    torqueMultiplier = float.Lerp(1, speedFactor, rpmFactor);
                }

                Input.ApplyTorque(-transmittedTorque, elapsed);
                Output.ApplyTorque(transmittedTorque * torqueMultiplier, elapsed);
            }
        }


        internal enum LockupResponseMode
        {
            Normal,
            Immediate,
        }
    }
}
