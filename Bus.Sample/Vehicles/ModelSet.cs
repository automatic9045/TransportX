using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Constraints;

using Bus.Common;
using Bus.Common.Physics;
using Bus.Common.Rendering;
using Bus.Common.Scenery;

namespace Bus.Sample.Vehicles
{
    internal class ModelSet : IDisposable
    {
        public required DynamicLocatedModel Body { get; init; }

        public required DynamicLocatedModel AxleF { get; init; }
        public required DynamicLocatedModel AxleR { get; init; }

        public required DynamicLocatedModel WheelFL { get; init; }
        public required DynamicLocatedModel WheelFR { get; init; }
        public required DynamicLocatedModel WheelRL { get; init; }
        public required DynamicLocatedModel WheelRR { get; init; }

        public required Constraint<Hinge> AxleToWheelFL { get; init; }
        public required Constraint<Hinge> AxleToWheelFR { get; init; }

        public required Constraint<AngularAxisMotor> PowerMotorRL { get; init; }
        public required Constraint<AngularAxisMotor> PowerMotorRR { get; init; }

        public required Constraint<AngularAxisMotor> BrakeMotorFL { get; init; }
        public required Constraint<AngularAxisMotor> BrakeMotorFR { get; init; }
        public required Constraint<AngularAxisMotor> BrakeMotorRL { get; init; }
        public required Constraint<AngularAxisMotor> BrakeMotorRR { get; init; }

        private ModelSet()
        {
        }

        public static ModelSet Create(Simulation simulation, LocatedModelCollection models, ModelFactory modelFactory)
        {
            CollidableModel bodyModel = modelFactory.WithConvexHull(@"Bus\Body.glb", true, new ColliderMaterial(0.6f, 2, new SpringSettings(30, 1)));
            Model wheelFLModelBase = modelFactory.NonCollision(@"Bus\WheelFL.glb", true);
            Model wheelRLModelBase = modelFactory.NonCollision(@"Bus\WheelRL.glb", true);

            DynamicLocatedModel body = models.Attach(bodyModel, Spec.Weight * 0.5f, SixDoF.Zero);

            Matrix4x4 wheelRotation = Matrix4x4.CreateRotationZ(float.Pi / 2); // Z軸奥向き正に見て左回転


            Cylinder axleFShape = new Cylinder(0.1f, 2.103f);
            Collider<Cylinder> axleFCollider = ColliderFactory.Cylinder(simulation, axleFShape, default, wheelRotation);
            CollidableModel axleFModel = new CollidableModel(axleFCollider);

            Cylinder axleRShape = new Cylinder(0.1f, 1.81f);
            Collider<Cylinder> axleRCollider = ColliderFactory.Cylinder(simulation, axleRShape, default, wheelRotation);
            CollidableModel axleRModel = new CollidableModel(axleRCollider);

            DynamicLocatedModel axleF = models.Attach(axleFModel, 400, ColliderGroupHandle.Skip, new SixDoF(0, 0.4756f, -2.346f));
            DynamicLocatedModel axleR = models.Attach(axleFModel, 700, ColliderGroupHandle.Skip, new SixDoF(0, 0.4756f, -7.226f));


            ColliderMaterial wheelMaterial = new ColliderMaterial(1, 0.5f, new SpringSettings(30, 1));

            Cylinder wheelFShape = new Cylinder(0.48f, 0.277f);
            Collider<Cylinder> wheelFCollider = ColliderFactory.Cylinder(simulation, wheelFShape, wheelMaterial, wheelRotation);
            CollidableModel wheelFLModel = new CollidableModel(wheelFLModelBase, wheelFCollider);

            Cylinder wheelRShape = new Cylinder(0.48f, 0.57f);
            Collider<Cylinder> wheelRCollider = ColliderFactory.Cylinder(simulation, wheelRShape, wheelMaterial, wheelRotation);
            CollidableModel wheelRLModel = new CollidableModel(wheelRLModelBase, wheelRCollider);

            DynamicLocatedModel wheelFL = models.Attach(wheelFLModel, 100, new SixDoF(-1.0515f, 0.4756f, -2.346f));
            DynamicLocatedModel wheelFR = models.Attach(wheelFLModel, 100, new SixDoF(1.0515f, 0.4756f, -2.346f, 0, float.Pi, 0));
            DynamicLocatedModel wheelRL = models.Attach(wheelRLModel, 280, new SixDoF(-0.905f, 0.4756f, -7.226f));
            DynamicLocatedModel wheelRR = models.Attach(wheelRLModel, 280, new SixDoF(0.905f, 0.4756f, -7.226f, 0, float.Pi, 0));


            Constraint<Hinge> axleToWheelFL = ConnectAxleToWheel(axleF, wheelFL);
            Constraint<Hinge> axleToWheelFR = ConnectAxleToWheel(axleF, wheelFR);
            ConnectAxleToWheel(axleR, wheelRL);
            ConnectAxleToWheel(axleR, wheelRR);

            Constraint<Hinge> ConnectAxleToWheel(DynamicLocatedModel axle, DynamicLocatedModel wheel)
            {
                Matrix4x4 baseToAxle = axle.BaseToCollider;
                Matrix4x4 baseToWheel = wheel.BaseToCollider;
                Matrix4x4 wheelToBase = wheel.ColliderToBase;
                Hinge hinge = new Hinge()
                {
                    LocalOffsetA = (wheelToBase * baseToAxle).Translation,
                    LocalOffsetB = Vector3.Zero,
                    LocalHingeAxisA = Vector3.TransformNormal(Vector3.UnitX, baseToAxle),
                    LocalHingeAxisB = Vector3.TransformNormal(Vector3.UnitX, baseToWheel),
                    SpringSettings = new SpringSettings(30, 1),
                };

                ConstraintHandle handle = simulation.Solver.Add(axle.Handle, wheel.Handle, hinge);
                return new Constraint<Hinge>(simulation, handle);
            }


            ConnectBodyToAxle(body, axleF, 7);
            ConnectBodyToAxle(body, axleR, 5);

            void ConnectBodyToAxle(DynamicLocatedModel body, DynamicLocatedModel axle, float springFrequency)
            {
                Matrix4x4 baseToBody = body.BaseToCollider;
                Matrix4x4 baseToAxle = axle.BaseToCollider;
                Matrix4x4 axleToBase = axle.ColliderToBase;

                Vector3 axleInBody = (axleToBase * baseToBody).Translation;
                Vector3 springOffsetInBody = Vector3.TransformNormal(Vector3.UnitX, baseToBody);
                Vector3 springOffsetInAxle = Vector3.TransformNormal(Vector3.UnitX, baseToAxle);
                Vector3 springDirectionInBody = Vector3.TransformNormal(Vector3.UnitY, baseToBody);

                PointOnLineServo track = new PointOnLineServo()
                {
                    LocalOffsetA = axleInBody,
                    LocalOffsetB = Vector3.Zero,
                    LocalDirection = springDirectionInBody,
                    ServoSettings = ServoSettings.Default,
                    SpringSettings = new SpringSettings(30, 1),
                };
                simulation.Solver.Add(body.Handle, axle.Handle, track);

                LinearAxisServo springL = new LinearAxisServo()
                {
                    LocalPlaneNormal = springDirectionInBody,
                    TargetOffset = 0,
                    LocalOffsetA = axleInBody - springOffsetInBody,
                    LocalOffsetB = -springOffsetInAxle,
                    ServoSettings = ServoSettings.Default,
                    SpringSettings = new SpringSettings(springFrequency, 1),
                };
                simulation.Solver.Add(body.Handle, axle.Handle, springL);

                LinearAxisServo springR = new LinearAxisServo()
                {
                    LocalPlaneNormal = springDirectionInBody,
                    TargetOffset = 0,
                    LocalOffsetA = axleInBody + springOffsetInBody,
                    LocalOffsetB = springOffsetInAxle,
                    ServoSettings = ServoSettings.Default,
                    SpringSettings = new SpringSettings(springFrequency, 1),
                };
                simulation.Solver.Add(body.Handle, axle.Handle, springR);

                AngularHinge stabilizer = new AngularHinge()
                {
                    LocalHingeAxisA = Vector3.TransformNormal(Vector3.UnitZ, baseToBody),
                    LocalHingeAxisB = Vector3.TransformNormal(Vector3.UnitZ, baseToAxle),
                    SpringSettings = new SpringSettings(30, 1),
                };
                simulation.Solver.Add(body.Handle, axle.Handle, stabilizer);
            }


            Constraint<AngularAxisMotor> powerMotorRL = AddMotor(axleR, wheelRL);
            Constraint<AngularAxisMotor> powerMotorRR = AddMotor(axleR, wheelRR);

            Constraint<AngularAxisMotor> brakeMotorFL = AddMotor(axleF, wheelFL);
            Constraint<AngularAxisMotor> brakeMotorFR = AddMotor(axleF, wheelFR);
            Constraint<AngularAxisMotor> brakeMotorRL = AddMotor(axleR, wheelRL);
            Constraint<AngularAxisMotor> brakeMotorRR = AddMotor(axleR, wheelRR);

            Constraint<AngularAxisMotor> AddMotor(DynamicLocatedModel axle, DynamicLocatedModel wheel)
            {
                AngularAxisMotor motor = new AngularAxisMotor()
                {
                    LocalAxisA = Vector3.TransformNormal(-Vector3.UnitX, axle.BaseToCollider),
                    TargetVelocity = 0,
                    Settings = new MotorSettings(0, 0),
                };

                ConstraintHandle handle = simulation.Solver.Add(axle.Handle, wheel.Handle, motor);
                return new Constraint<AngularAxisMotor>(simulation, handle);
            }


            return new ModelSet()
            {
                Body = body,

                AxleF = axleF,
                AxleR = axleR,

                WheelFL = wheelFL,
                WheelFR = wheelFR,
                WheelRL = wheelRL,
                WheelRR = wheelRR,

                AxleToWheelFL = axleToWheelFL,
                AxleToWheelFR = axleToWheelFR,

                PowerMotorRL = powerMotorRL,
                PowerMotorRR = powerMotorRR,

                BrakeMotorFL = brakeMotorFL,
                BrakeMotorFR = brakeMotorFR,
                BrakeMotorRL = brakeMotorRL,
                BrakeMotorRR = brakeMotorRR,
            };
        }

        public void Dispose()
        {
            Body.Model.Dispose();
            WheelFL.Model.Dispose();
            WheelRL.Model.Dispose();
        }
    }
}
