using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Constraints;

using TransportX;
using TransportX.Bodies;
using TransportX.Physics;
using TransportX.Rendering;
using TransportX.Spatial;

namespace TransportX.Sample.LV290.Vehicles
{
    internal class ModelSet : IDisposable
    {
        private readonly List<IDisposable> Disposables = [];

        public DynamicLocatedModel Body { get; }

        public KinematicLocatedModel FrontDoor1 { get; }
        public KinematicLocatedModel FrontDoor2 { get; }
        public KinematicLocatedModel RearDoor { get; }

        public DynamicLocatedModel AxleF { get; }
        public DynamicLocatedModel AxleR { get; }

        public DynamicLocatedModel WheelFL { get; }
        public DynamicLocatedModel WheelFR { get; }
        public DynamicLocatedModel WheelRL { get; }
        public DynamicLocatedModel WheelRR { get; }

        public Constraint<Hinge> AxleToWheelFL { get; }
        public Constraint<Hinge> AxleToWheelFR { get; }

        public Constraint<AngularAxisMotor> PowerMotorRL { get; }
        public Constraint<AngularAxisMotor> PowerMotorRR { get; }

        public Constraint<AngularAxisMotor> BrakeMotorFL { get; }
        public Constraint<AngularAxisMotor> BrakeMotorFR { get; }
        public Constraint<AngularAxisMotor> BrakeMotorRL { get; }
        public Constraint<AngularAxisMotor> BrakeMotorRR { get; }

        public ModelSet(Simulation simulation, BodyStructure structure, ModelFactory modelFactory)
        {
            ColliderMaterial bodyMaterial = new(0.6f, 2, new SpringSettings(30, 1));
            ColliderMaterial wheelMaterial = new(1, 0.5f, new SpringSettings(30, 1));

            CollidableModel bodyModel = modelFactory.WithConvexHull(@"Models\LV290N.glb", true, bodyMaterial);
            Disposables.Add(bodyModel);

            CollidableModel frontDoor1Model = modelFactory.WithBoundingBox(@"Models\FrontDoor1.glb", true, bodyMaterial);
            CollidableModel frontDoor2Model = modelFactory.WithBoundingBox(@"Models\FrontDoor2.glb", true, bodyMaterial);
            CollidableModel rearDoorModel = modelFactory.WithBoundingBox(@"Models\RearDoor.glb", true, bodyMaterial);
            Disposables.Add(frontDoor1Model);
            Disposables.Add(frontDoor2Model);
            Disposables.Add(rearDoorModel);

            Model wheelFLModelBase = modelFactory.NonCollision(@"Models_Kuusemi\WheelFL.glb", true);
            Model wheelRLModelBase = modelFactory.NonCollision(@"Models_Kuusemi\WheelRL.glb", true);

            Body = structure.AttachDynamic(bodyModel, Spec.Weight * 0.5f, SixDoF.Zero);

            FrontDoor1 = structure.AttachKinematic(frontDoor1Model, new SixDoF(-1.18f, 0, -0.4f));
            FrontDoor2 = structure.AttachKinematic(frontDoor2Model, new SixDoF(-1.18f, 0, -1.42f));
            RearDoor = structure.AttachKinematic(rearDoorModel, SixDoF.Zero);

            Pose wheelRotation = Pose.CreateRotationZ(float.Pi / 2); // Z軸奥向き正に見て左回転


            Cylinder axleFShape = new(0.1f, 2.103f);
            ColliderBase<Cylinder> axleFCollider = ColliderFactory.Cylinder(simulation, axleFShape, default, wheelRotation);
            CollidableModel axleFModel = new(axleFCollider);
            Disposables.Add(axleFModel);

            Cylinder axleRShape = new(0.1f, 1.81f);
            ColliderBase<Cylinder> axleRCollider = ColliderFactory.Cylinder(simulation, axleRShape, default, wheelRotation);
            CollidableModel axleRModel = new(axleRCollider);
            Disposables.Add(axleRModel);

            AxleF = structure.AttachDynamic(axleFModel, 400, ColliderGroupHandle.Skip, new SixDoF(0, 0.4756f, -Spec.FrontOverhang));
            AxleR = structure.AttachDynamic(axleFModel, 700, ColliderGroupHandle.Skip, new SixDoF(0, 0.4756f, -Spec.FrontOverhang - Spec.Wheelbase));


            Cylinder wheelFShape = new(0.48f, 0.277f);
            CollidableModel wheelFLModel = modelFactory.WithCylinder(wheelFLModelBase, wheelFShape, wheelMaterial, wheelRotation);
            Disposables.Add(wheelFLModel);

            Cylinder wheelRShape = new(0.48f, 0.57f);
            CollidableModel wheelRLModel = modelFactory.WithCylinder(wheelRLModelBase, wheelRShape, wheelMaterial, wheelRotation);
            Disposables.Add(wheelRLModel);

            WheelFL = structure.AttachDynamic(wheelFLModel, 100, new SixDoF(-1.0515f, 0.4756f, -Spec.FrontOverhang));
            WheelFR = structure.AttachDynamic(wheelFLModel, 100, new SixDoF(1.0515f, 0.4756f, -Spec.FrontOverhang, 0, float.Pi, 0));
            WheelRL = structure.AttachDynamic(wheelRLModel, 280, new SixDoF(-0.905f, 0.4756f, -Spec.FrontOverhang - Spec.Wheelbase));
            WheelRR = structure.AttachDynamic(wheelRLModel, 280, new SixDoF(0.905f, 0.4756f, -Spec.FrontOverhang - Spec.Wheelbase, 0, float.Pi, 0));


            AxleToWheelFL = ConnectAxleToWheel(AxleF, WheelFL);
            AxleToWheelFR = ConnectAxleToWheel(AxleF, WheelFR);
            ConnectAxleToWheel(AxleR, WheelRL);
            ConnectAxleToWheel(AxleR, WheelRR);

            Constraint<Hinge> ConnectAxleToWheel(DynamicLocatedModel axle, DynamicLocatedModel wheel)
            {
                Pose baseToAxle = axle.BaseToCollider;
                Pose baseToWheel = wheel.BaseToCollider;
                Pose wheelToBase = wheel.ColliderToBase;
                Hinge hinge = new()
                {
                    LocalOffsetA = (wheelToBase * baseToAxle).Position,
                    LocalOffsetB = Vector3.Zero,
                    LocalHingeAxisA = Pose.TransformNormal(Vector3.UnitX, baseToAxle),
                    LocalHingeAxisB = Pose.TransformNormal(Vector3.UnitX, baseToWheel),
                    SpringSettings = new SpringSettings(30, 1),
                };

                ConstraintHandle handle = simulation.Solver.Add(axle.Handle, wheel.Handle, hinge);
                return new Constraint<Hinge>(simulation, handle);
            }


            ConnectBodyToAxle(Body, AxleF, 7);
            ConnectBodyToAxle(Body, AxleR, 5);

            void ConnectBodyToAxle(DynamicLocatedModel body, DynamicLocatedModel axle, float springFrequency)
            {
                Pose baseToBody = body.BaseToCollider;
                Pose baseToAxle = axle.BaseToCollider;
                Pose axleToBase = axle.ColliderToBase;

                Vector3 axleInBody = (axleToBase * baseToBody).Position;
                Vector3 springOffsetInBody = Pose.TransformNormal(Vector3.UnitX, baseToBody);
                Vector3 springOffsetInAxle = Pose.TransformNormal(Vector3.UnitX, baseToAxle);
                Vector3 springDirectionInBody = Pose.TransformNormal(Vector3.UnitY, baseToBody);

                PointOnLineServo track = new()
                {
                    LocalOffsetA = axleInBody,
                    LocalOffsetB = Vector3.Zero,
                    LocalDirection = springDirectionInBody,
                    ServoSettings = ServoSettings.Default,
                    SpringSettings = new SpringSettings(30, 1),
                };
                simulation.Solver.Add(body.Handle, axle.Handle, track);

                LinearAxisServo springL = new()
                {
                    LocalPlaneNormal = springDirectionInBody,
                    TargetOffset = 0,
                    LocalOffsetA = axleInBody - springOffsetInBody,
                    LocalOffsetB = -springOffsetInAxle,
                    ServoSettings = ServoSettings.Default,
                    SpringSettings = new SpringSettings(springFrequency, 1),
                };
                simulation.Solver.Add(body.Handle, axle.Handle, springL);

                LinearAxisServo springR = new()
                {
                    LocalPlaneNormal = springDirectionInBody,
                    TargetOffset = 0,
                    LocalOffsetA = axleInBody + springOffsetInBody,
                    LocalOffsetB = springOffsetInAxle,
                    ServoSettings = ServoSettings.Default,
                    SpringSettings = new SpringSettings(springFrequency, 1),
                };
                simulation.Solver.Add(body.Handle, axle.Handle, springR);

                AngularHinge stabilizer = new()
                {
                    LocalHingeAxisA = Pose.TransformNormal(Vector3.UnitZ, baseToBody),
                    LocalHingeAxisB = Pose.TransformNormal(Vector3.UnitZ, baseToAxle),
                    SpringSettings = new SpringSettings(30, 1),
                };
                simulation.Solver.Add(body.Handle, axle.Handle, stabilizer);
            }


            PowerMotorRL = AddMotor(AxleR, WheelRL);
            PowerMotorRR = AddMotor(AxleR, WheelRR);

            BrakeMotorFL = AddMotor(AxleF, WheelFL);
            BrakeMotorFR = AddMotor(AxleF, WheelFR);
            BrakeMotorRL = AddMotor(AxleR, WheelRL);
            BrakeMotorRR = AddMotor(AxleR, WheelRR);

            Constraint<AngularAxisMotor> AddMotor(DynamicLocatedModel axle, DynamicLocatedModel wheel)
            {
                AngularAxisMotor motor = new()
                {
                    LocalAxisA = Pose.TransformNormal(-Vector3.UnitX, axle.BaseToCollider),
                    TargetVelocity = 0,
                    Settings = new MotorSettings(0, 0),
                };

                ConstraintHandle handle = simulation.Solver.Add(axle.Handle, wheel.Handle, motor);
                return new Constraint<AngularAxisMotor>(simulation, handle);
            }
        }

        public void Dispose()
        {
            foreach (IDisposable disposable in Disposables)
            {
                disposable.Dispose();
            }
        }
    }
}
