using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common;

using Bus.Sample.Vehicles.Interfaces;

namespace Bus.Sample.Vehicles.Drives
{
    internal class Tire
    {
        private const double WheelRadius = 0.959 / 2;
        private const double InnerSteeringAngle = 53 / 180d * double.Pi;
        private const double OuterSteeringAngle = 38.5 / 180d * double.Pi;

        private const double CorneringStiffness = 200000;
        private const double LongitudinalSlipStiffness = 400000;
        private const double RollingResistanceCoeff = 0.005;
        private const double ContactPatchLength = 0.3;

        private const double Inertia = 1;

        public const double Thickness = 0.2;


        private readonly Steering Steering;
        private readonly AttachableObject Location;

        public Vector3 Position { get; }
        public SlipMode Mode { get; }

        public double FrictionCoeff { get; set; } = 1;
        public double VerticalLoad { get; set; } = Spec.Weight / 6 * Constants.G;

        public double LongitudinalForce { get; private set; } = 0;
        public double RollingResistanceMoment { get; private set; } = 0;

        public double LateralForce { get; private set; } = 0;
        public double SelfAligningTorque { get; private set; } = 0;

        public Vector3 Moment => new Vector3(0, 0, 0);

        public double SlipAngle { get; private set; } = 0;

        public double AngularAcceleration { get; private set; } = 0;
        public double AngularVelocity { get; private set; } = 0;
        public double Angle { get; private set; } = 0;
        public double Velocity => AngularVelocity * WheelRadius;

        public Tire(Vector3 position, SlipMode mode, Steering steering)
        {
            Position = position;
            Mode = mode;
            Steering = steering;
        }

        public void ComputeTick(double driveTorque, double vehicleVelocity, TimeSpan elapsed)
        {
            if (double.IsNaN(vehicleVelocity))
            {
                LongitudinalForce = 0;
                RollingResistanceMoment = 0;
                LateralForce = 0;
                SelfAligningTorque = 0;
            }
            else
            {
                if (vehicleVelocity < 0.001 && false)
                    LongitudinalForce = 0;
                else
                {
                    double slipRatio = Velocity / double.Max(vehicleVelocity, 0.001) - 1;
                    double absSlipRatio = double.Abs(slipRatio);
                    double criticalSlipRatio = 3 * FrictionCoeff * VerticalLoad / LongitudinalSlipStiffness;

                    if (absSlipRatio < criticalSlipRatio)
                    {
                        LongitudinalForce = LongitudinalSlipStiffness * slipRatio
                            * (1 - absSlipRatio / criticalSlipRatio + slipRatio * slipRatio / 3 / criticalSlipRatio / criticalSlipRatio);
                    }
                    else
                    {
                        LongitudinalForce = double.Sign(slipRatio) * FrictionCoeff * VerticalLoad;
                    }
                }

                RollingResistanceMoment = -double.Sign(AngularVelocity) * RollingResistanceCoeff * VerticalLoad * WheelRadius;


                int modeDirection = Mode == SlipMode.Left ? -1 : Mode == SlipMode.Right ? 1 : 0;
                double maxSlipAngle = modeDirection == 0 ? 0 : modeDirection == double.Sign(Steering.Rate) ? InnerSteeringAngle : OuterSteeringAngle;
                SlipAngle = maxSlipAngle * Steering.Rate;

                double tanSlipAngle = double.Tan(SlipAngle);
                double absTanSlipAngle = double.Abs(tanSlipAngle);
                double tanCriticalSlipAngle = 3 * FrictionCoeff * VerticalLoad / CorneringStiffness;

                if (absTanSlipAngle < tanCriticalSlipAngle)
                {
                    LateralForce = CorneringStiffness * tanSlipAngle
                        * (-1 + absTanSlipAngle / tanCriticalSlipAngle - tanSlipAngle * tanSlipAngle / 3 / tanCriticalSlipAngle / tanCriticalSlipAngle);

                    SelfAligningTorque = ContactPatchLength * CorneringStiffness / 6 * tanSlipAngle
                        * (1 - 3 * absTanSlipAngle / tanCriticalSlipAngle + tanSlipAngle * tanSlipAngle / tanCriticalSlipAngle / tanCriticalSlipAngle);
                }
                else
                {
                    LateralForce = -double.Sign(SlipAngle) * FrictionCoeff * VerticalLoad;
                    SelfAligningTorque = 0;
                }
            }

            double brakeTorque = 0;
            double netTorque = driveTorque - brakeTorque - LongitudinalForce * WheelRadius + RollingResistanceMoment;
            AngularAcceleration = netTorque / Inertia;
            AngularVelocity += AngularAcceleration * elapsed.TotalSeconds;
            Angle = (Angle + AngularVelocity * elapsed.TotalSeconds) % double.Tau;
        }


        internal enum SlipMode
        {
            None,
            Left,
            Right,
        }
    }
}
