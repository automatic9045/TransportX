using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Mathematics;

using TransportX.Domains.RoadVehicles.Physics;

namespace TransportX.Domains.RoadVehicles.Powertrain.Modules
{
    public class FluidClutch : ClutchBase
    {
        public required Curve CapacityFactorCurve { get; init; }
        public required Curve TorqueRatioCurve { get; init; }

        public required float FrictionCoefficient { get; init; }
        public required float FluidCoefficient { get; init; }
        public required float CoastFluidCoefficient { get; init; }

        public required float LockUpSpeed { get; init; }
        public required float ImmediateLockUpSpeed { get; init; }

        public bool Lockup { get; set; } = false;
        public LockupResponseMode LockupMode { get; set; } = LockupResponseMode.Normal;
        public float LockupRate { get; private set; } = 0;

        public FluidClutch(Shaft input, Shaft output) : base(input, output)
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

                float speedRatio = 0 < Input.AngularVelocity ? float.Clamp(Output.AngularVelocity / Input.AngularVelocity, 0, 1) : 0;

                float capacityFactor = CapacityFactorCurve.GetValue(speedRatio);
                float torqueRatio = TorqueRatioCurve.GetValue(speedRatio);

                float pumpTorque = capacityFactor * Input.AngularVelocity * Input.AngularVelocity;
                float turbineTorque = pumpTorque * torqueRatio;

                float delta = Input.AngularVelocity - Output.AngularVelocity;

                if (Input.AngularVelocity < Output.AngularVelocity)
                {
                    float coastDelta = Output.AngularVelocity - Input.AngularVelocity;
                    float coastTorque = CoastFluidCoefficient * coastDelta * coastDelta;
                    pumpTorque = -coastTorque;
                    turbineTorque = -coastTorque;
                }

                float frictionTorque = LockupRate * FrictionCoefficient * float.Sign(delta);
                pumpTorque += frictionTorque;
                turbineTorque += frictionTorque;

                float maxStabilizingTorque = float.Abs(delta) * float.Min(Input.Inertia, Output.Inertia) / (float)elapsed.TotalSeconds;
                float maxAbsTorque = float.Max(float.Abs(pumpTorque), float.Abs(turbineTorque));
                if (maxStabilizingTorque < maxAbsTorque && 0 < maxAbsTorque)
                {
                    float scale = maxStabilizingTorque / maxAbsTorque;
                    pumpTorque *= scale;
                    turbineTorque *= scale;
                }

                Input.ApplyTorque(-pumpTorque * Engagement, elapsed);
                Output.ApplyTorque(turbineTorque * Engagement, elapsed);
            }
        }


        public enum LockupResponseMode
        {
            Normal,
            Immediate,
        }
    }
}
