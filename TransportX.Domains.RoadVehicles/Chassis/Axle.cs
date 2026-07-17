using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Constraints;

using TransportX.Communication;
using TransportX.Mathematics;
using TransportX.Physics;
using TransportX.Spatial;

namespace TransportX.Domains.RoadVehicles.Chassis
{
    public class Axle
    {
        public required DynamicTransformedModel Beam { get; init; }
        public required DynamicTransformedModel WheelL { get; init; }
        public required DynamicTransformedModel WheelR { get; init; }

        public required Knuckle KnuckleL { get; init; }
        public required Knuckle KnuckleR { get; init; }

        public required Brake BrakeL { get; init; }
        public required Brake BrakeR { get; init; }

        public float WheelLSpeed
        {
            get
            {
                Vector3 angularVelocity = WheelL.Body.Velocity.Angular - Beam.Body.Velocity.Angular;
                Vector3 hingeAxis = Vector3.Transform(KnuckleL.Hinge.Description.LocalHingeAxisA, Beam.Body.Pose.Orientation);

                return Vector3.Dot(angularVelocity, hingeAxis);
            }
        }

        public float WheelRSpeed
        {
            get
            {
                Vector3 angularVelocity = WheelR.Body.Velocity.Angular - Beam.Body.Velocity.Angular;
                Vector3 hingeAxis = Vector3.Transform(KnuckleR.Hinge.Description.LocalHingeAxisA, Beam.Body.Pose.Orientation);

                return Vector3.Dot(angularVelocity, hingeAxis);
            }
        }

        public float WheelSpeed => (WheelLSpeed + WheelRSpeed) / 2;

        public Axle()
        {
        }

        public static Axle Create(Simulation simulation, DynamicTransformedModel beam, in Wheel wheelL, in Wheel wheelR)
        {
            (Knuckle knuckleL, Brake brakeL) = ConnectBeamToWheel(wheelL);
            (Knuckle knuckleR, Brake brakeR) = ConnectBeamToWheel(wheelR);

            return new Axle()
            {
                Beam = beam,
                WheelL = wheelL.Model,
                WheelR = wheelR.Model,

                KnuckleL = knuckleL,
                KnuckleR = knuckleR,

                BrakeL = brakeL,
                BrakeR = brakeR,
            };


            (Knuckle Knuckle, Brake Brake) ConnectBeamToWheel(in Wheel wheel)
            {
                Pose baseToBeam = beam.BaseToCollider;
                Pose baseToWheel = wheel.Model.BaseToCollider;
                Pose wheelToBase = wheel.Model.ColliderToBase;

                Hinge hinge = new()
                {
                    LocalOffsetA = (wheelToBase * baseToBeam).Position,
                    LocalOffsetB = Vector3.Zero,
                    LocalHingeAxisA = Pose.TransformNormal(Vector3.UnitX, baseToBeam),
                    LocalHingeAxisB = Pose.TransformNormal(Vector3.UnitX, baseToWheel),
                    SpringSettings = wheel.SpringSettings,
                };
                ConstraintHandle hingeHandle = simulation.Solver.Add(beam.Body, wheel.Model.Body, hinge);
                Constraint<Hinge> hingeConstraint = new(simulation, hingeHandle);
                Knuckle knuckle = new()
                {
                    Hinge = hingeConstraint,
                    BaseToBeam = baseToBeam,
                    SteeringSignal = wheel.SteeringSignal,
                    SteeringLeftAngle = wheel.SteeringLeftAngle,
                    SteeringRightAngle = wheel.SteeringRightAngle,
                };

                AngularAxisMotor motor = new()
                {
                    LocalAxisA = Pose.TransformNormal(-Vector3.UnitX, baseToBeam),
                    TargetVelocity = 0,
                    Settings = new MotorSettings(0, 0),
                };
                ConstraintHandle motorHandle = simulation.Solver.Add(beam.Body, wheel.Model.Body, motor);
                Constraint<AngularAxisMotor> motorConstraint = new(simulation, motorHandle);
                Brake brake = new()
                {
                    Motor = motorConstraint,
                    Signal = wheel.BrakeSignal,
                    TorqueCurve = wheel.BrakeTorqueCurve,
                };

                return (knuckle, brake);
            }
        }

        public void Tick(TimeSpan elapsed)
        {
            KnuckleL.Update();
            KnuckleR.Update();

            BrakeL.Update();
            BrakeR.Update();
        }


        public readonly record struct Wheel(
            DynamicTransformedModel Model, SpringSettings SpringSettings,
            Signal<float>? SteeringSignal, float SteeringLeftAngle, float SteeringRightAngle,
            Signal<float>? BrakeSignal, Curve BrakeTorqueCurve);
    }
}
