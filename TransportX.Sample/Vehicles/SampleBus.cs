using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using BepuPhysics.Constraints;

using TransportX;
using TransportX.Avatars;
using TransportX.Dependency;
using TransportX.Input;
using TransportX.Network;
using TransportX.Physics;
using TransportX.Rendering;
using TransportX.Spatial;
using TransportX.Traffic;

using TransportX.Sample.Vehicles.Audio;
using TransportX.Sample.Vehicles.Doors;
using TransportX.Sample.Vehicles.Input;
using TransportX.Sample.Vehicles.Interfaces;
using TransportX.Sample.Vehicles.Powertrain;

namespace TransportX.Sample.Vehicles
{
    [AvatarIdentifier("Sample")]
    public class SampleBus : AvatarBase
    {
        private readonly ModelSet BusModels;
        private readonly IReadOnlyList<IInput> Inputs;
        private readonly InterfaceSet Interfaces;
        private readonly PowertrainSet Powertrain;

        private readonly BifoldDoor FrontDoor;
        private readonly PocketDoor RearDoor;

        private readonly AudioSet Audios;

        private readonly KeyObserver ResetKey;

        public override string Title { get; } = "サンプルバス";
        public override string Description { get; } = "動作確認用のバスです。";
        public override string Author { get; } = "automatic9045";

        public override Viewpoint DriverViewpoint { get; }
        public override Viewpoint BirdViewpoint { get; }

        public override float Width => Spec.Width;
        public override float Height => Spec.Height;
        public override float Length => Spec.Length;

        public override bool IsEnabled => true;
        public override ILanePath? Path => null;
        public override ParticipantDirection Heading => ParticipantDirection.Forward;
        public override float S => 0;
        public override float SVelocity => 0;

        public SampleBus(PluginLoadContext context, AvatarBuilder builder) : base(context, builder)
        {
            ResetKey = InputManager.ObserveKey(Key.R);

            ModelFactory modelFactory = new(DXHost.Context, PhysicsHost.Simulation, ErrorCollector, new Vector4(0, 1, 0, 1));
            SoundFactory soundFactory = new(DXHost.XAudio2, DXHost.MasteringVoice, DXHost.X3DAudio, this);
            BusModels = new ModelSet(PhysicsHost.Simulation, Structure, modelFactory);
            Inputs = [new KeyboardInput(InputManager, () => Vector3.Dot(Velocity, Pose.Direction))];
            Interfaces = new InterfaceSet(Inputs, Inputs[0]);
            Powertrain = new PowertrainSet(Interfaces, BusModels.WheelRL, BusModels.WheelRR, BusModels.PowerMotorRL, BusModels.PowerMotorRR);

            FrontDoor = new BifoldDoor(Interfaces.DoorSwitch, BusModels.FrontDoor1, BusModels.FrontDoor2);
            RearDoor = new PocketDoor(Interfaces.DoorSwitch, BusModels.RearDoor);

            Audios = new AudioSet(soundFactory, Powertrain.Engine, FrontDoor, RearDoor);

            DriverViewpoint = new DriverViewpoint(this, new SixDoF(0.67f, 1.8f, -1.3f));
            BirdViewpoint = new BirdViewpoint(this, new SixDoF(0, 2, -3), 20, new Vector2(0.3f, 0));
        }

        public override void Dispose()
        {
            base.Dispose();
            BusModels.Dispose();
            Audios.Dispose();
        }

        public override bool Spawn(ILanePath path, ParticipantDirection heading, float s)
        {
            throw new NotSupportedException();
        }

        public override void SubTick(TimeSpan elapsed)
        {
            if (ResetKey.IsPressed)
            {
                Locate(0, 0, new SixDoF(10, 1f, 25, 0, 0, 0.01f));
                foreach (LocatedModel model in Structure)
                {
                    if (model is DynamicLocatedModel dynamicModel) dynamicModel.Body.Velocity = default;
                    model.Pose = model.BasePose * Pose;
                }
            }

            base.SubTick(elapsed);

            Powertrain.Tick(elapsed);

            Audios.UpdateSound(Camera);


            float wheelRate = Interfaces.SteeringWheel.Rate / Spec.MaxSteeringWheelAngle;
            Steer(BusModels.AxleToWheelFL, wheelRate, -1);
            Steer(BusModels.AxleToWheelFR, wheelRate, 1);

            void Steer(Constraint<Hinge> hinge, float rate, int direction)
            {
                float angle = (float.Sign(rate) == direction ? Spec.InnerSteeringAngle : Spec.OuterSteeringAngle) * rate;
                Quaternion rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, angle);
                Vector3 axis = Vector3.Transform(Vector3.UnitX, rotation);
                hinge.Update(desc =>
                {
                    desc.LocalHingeAxisA = Pose.TransformNormal(axis, BusModels.AxleF.BaseToCollider);
                    return desc;
                });
            }


            float brakeTorque = Interfaces.Brake.Rate * 2000 + 285;
            ApplyBrakeTorque(BusModels.WheelFL, BusModels.BrakeMotorFL, brakeTorque);
            ApplyBrakeTorque(BusModels.WheelFR, BusModels.BrakeMotorFR, brakeTorque);
            ApplyBrakeTorque(BusModels.WheelRL, BusModels.BrakeMotorRL, brakeTorque);
            ApplyBrakeTorque(BusModels.WheelRR, BusModels.BrakeMotorRR, brakeTorque);

            void ApplyBrakeTorque(DynamicLocatedModel wheel, Constraint<AngularAxisMotor> motor, float torque)
            {
                motor.Update(desc =>
                {
                    desc.Settings.MaximumForce = brakeTorque;
                    return desc;
                });
            }
        }

        public override void Tick(TimeSpan elapsed)
        {
            foreach (IInput input in Inputs) input.Tick(elapsed);
            Interfaces.Tick(BusModels.WheelRL.AngularVelocity.Length(), elapsed);

            FrontDoor.Tick(elapsed);
            RearDoor.Tick(elapsed);

            //Drives.Tick(elapsed);

            //Drives.UpdateSound(DXHost.X3DAudio, Camera.Listener, Camera.PlateX, Camera.PlateZ);

            Application.Current.MainWindow.Title =
                $"[{TimeManager.Fps:f0} fps] " +
                $"{Powertrain.Engine.Rpm:f0}rpm, [G{Powertrain.Transmission.Gear}; cl{Powertrain.Clutch.Engagement:f2}; th{Powertrain.Engine.ECU.Throttle:f2}], " +
                //$"ω={Drives.LeftWheel.AngularVelocity:f2};{Drives.RightWheel.AngularVelocity:f2}, " +
                //$"Te={Powertrain.Engine.Torque:f0}, Ttc={Powertrain.Clutch.OutTorque:f0}, Ttr={Powertrain.Transmission.OutTorque:f0}, " +
                //$"r={Position:f1}, " +
                //$"v={Powertrain.LeftWheel.Velocity * 3.6f:f1} km/h, {Powertrain.LeftWheel.Rpm:f1}rpm";
                $"Tl={Powertrain.LeftWheel.OutTorque:f1} Nm, " +
                $"rs={Interfaces.SteeringWheel.Rate:f2}, " +
                $"v={Vector3.Dot(Velocity, Pose.Direction) * 3.6f:f1} km/h";

            /*Tire tire = Drives.Chassis.Tires[2];
            Application.Current.MainWindow.Title =
                $"Tt={Drives.Transmission.Torque:F1}; " +
                $"M=a={tire.AngularAcceleration:F1}, v={tire.AngularVelocity:F1}; " +
                $"Fx={tire.LateralForce:F1}, Fy={tire.VerticalLoad:F1}, Fz={tire.LongitudinalForce:F1}; " +
                $"Mx={tire.RollingResistanceMoment:F1}, My={tire.SelfAligningTorque:F1}";*/
        }
    }
}
