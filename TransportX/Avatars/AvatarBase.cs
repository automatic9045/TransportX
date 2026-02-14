using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Bodies;
using TransportX.Dependency;
using TransportX.Diagnostics;
using TransportX.Input;
using TransportX.Network;
using TransportX.Physics;
using TransportX.Rendering;
using TransportX.Spatial;
using TransportX.Traffic;
using TransportX.Worlds;

namespace TransportX.Avatars
{
    public abstract class AvatarBase : RigidBody, ITrafficParticipant
    {
        private readonly IDebugModel DebugModel;

        public abstract string Title { get; }
        public abstract string Description { get; }
        public abstract string Author { get; }

        public IDXHost DXHost { get; }
        public IDXClient DXClient { get; }
        public IPhysicsHost PhysicsHost { get; }
        public IErrorCollector ErrorCollector { get; }
        public PluginLoadContext GameContext { get; }
        public PluginLoadContext WorldContext { get; }
        public PluginLoadContext AvatarContext { get; }
        public ITimeManager TimeManager { get; }
        public InputManager InputManager { get; }
        public Camera Camera { get; }
        public WorldBase World { get; }

        public abstract Viewpoint DriverViewpoint { get; }
        public abstract Viewpoint BirdViewpoint { get; }

        public abstract float Width { get; }
        public abstract float Height { get; }
        public abstract float Length { get; }

        public abstract bool IsEnabled { get; }
        public abstract ILanePath? Path { get; }
        public abstract ParticipantDirection Heading { get; }
        public abstract float S { get; }
        public abstract float SVelocity { get; }
        float ITrafficParticipant.SVelocity => SVelocity;

        protected Vector4 DebugColor
        {
            get => DebugModel.Color;
            set => DebugModel.Color = value;
        }

        public AvatarBase(PluginLoadContext context, AvatarBuilder builder) : base(builder.PhysicsHost)
        {
            DXHost = builder.DXHost;
            DXClient = builder.DXClient;
            PhysicsHost = builder.PhysicsHost;
            ErrorCollector = builder.ErrorCollector;
            GameContext = builder.GameContext;
            WorldContext = builder.WorldContext;
            AvatarContext = context;
            TimeManager = builder.TimeManager;
            InputManager = builder.InputManager;
            Camera = builder.Camera;
            World = builder.World;

            DebugModel = this.CreateDebugModel(DXHost.Device);
            DebugModel.DebugName = GetType().Name;
            DebugModel.Color = new Vector4(1, 0, 0, 1);
        }

        public override void Dispose()
        {
            base.Dispose();
            DebugModel.Dispose();
        }

        public new PlateOffset TeleportTo(int plateX, int plateZ, Pose pose)
        {
            return base.TeleportTo(plateX, plateZ, pose);
        }

        public abstract bool Spawn(ILanePath path, ParticipantDirection heading, float s);

        public override void Draw(LocatedDrawContext context)
        {
            base.Draw(context);

            if (context.Pass == RenderPass.Traffic)
            {
                TransformBuffer transformData = new()
                {
                    World = Matrix4x4.Transpose((Pose * context.PlateOffset.Pose).ToMatrix4x4()),
                    View = Matrix4x4.Transpose(context.View),
                    Projection = Matrix4x4.Transpose(context.Projection),
                };
                context.DeviceContext.UpdateSubresource(transformData, context.TransformBuffer);

                DebugModel.Draw(new(context.DeviceContext, context.TransformBuffer, context.MaterialBuffer));
            }
        }
    }
}
