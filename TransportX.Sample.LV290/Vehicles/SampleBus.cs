using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics.Constraints;
using Silk.NET.Input;

using TransportX.Avatars;
using TransportX.Cameras;
using TransportX.Dependency;
using TransportX.Input;
using TransportX.Network;
using TransportX.Physics;
using TransportX.Spatial;
using TransportX.Traffic;

using TransportX.Sample.LV290.Vehicles.Audio;
using TransportX.Sample.LV290.Vehicles.Doors;
using TransportX.Sample.LV290.Vehicles.Input;
using TransportX.Sample.LV290.Vehicles.Interfaces;
using TransportX.Sample.LV290.Vehicles.Powertrain;

namespace TransportX.Sample.LV290.Vehicles
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

        private bool IsInitialWorldPoseSet = false;
        private WorldPose InitialWorldPose = WorldPose.Zero;

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
        public override EntityDirection Heading => EntityDirection.Forward;
        public override float S => 0;
        public override float SVelocity => 0;

        public SampleBus(PluginLoadContext context, AvatarBuilder builder) : base(context, builder)
        {
            ResetKey = InputManager.ObserveKey(Key.R);

            ModelFactory modelFactory = new(DXHost.Context, PhysicsHost.Simulation, ErrorCollector, new Vector4(0, 1, 0, 1));
            SoundFactory soundFactory = new(DXHost.XAudio2, DXHost.MasteringVoice, DXHost.X3DAudio, this);
            BusModels = new ModelSet(PhysicsHost.Simulation, Structure, modelFactory);
            Inputs = [new KeyboardInput(InputManager, () => Vector3.Dot(Velocity, WorldPose.Pose.Direction))];
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

        public override bool Spawn(ILanePath path, EntityDirection heading, float s)
        {
            throw new NotSupportedException();
        }

        public override void SubTick(TimeSpan elapsed)
        {
            if (!IsInitialWorldPoseSet)
            {
                InitialWorldPose = WorldPose;
                IsInitialWorldPoseSet = true;
            }
            if (ResetKey.IsPressed)
            {
                Locate(InitialWorldPose);

                foreach (TransformedModel model in Structure)
                {
                    if (model is DynamicTransformedModel dynamicModel) dynamicModel.Body.Velocity = default;
                    model.Pose = model.BasePose * WorldPose.Pose;
                }
            }

            ChunkIndex offset = Camera.WorldPose.Chunk - WorldPose.Chunk;
            if (World.Options.SimulationChunkCount <= int.Abs(offset.X) || World.Options.SimulationChunkCount <= int.Abs(offset.Z))
            {
                Structure.Freeze();
                return;
            }
            else
            {
                Structure.Unfreeze();
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

            void ApplyBrakeTorque(DynamicTransformedModel wheel, Constraint<AngularAxisMotor> motor, float torque)
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

            string gearText = Powertrain.Transmission.Gear switch
            {
                -1 => "R",
                0 => "N",
                _ => Powertrain.Transmission.Gear.ToString(),
            };
            Platform.Window.Title += $"; [Gear {gearText}] {Powertrain.Engine.Rpm:f0} rpm, {Vector3.Dot(Velocity, WorldPose.Pose.Direction) * 3.6f:f1} km/h";
        }
    }
}
