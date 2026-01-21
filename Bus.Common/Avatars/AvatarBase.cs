using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Bodies;
using Bus.Common.Dependency;
using Bus.Common.Diagnostics;
using Bus.Common.Input;
using Bus.Common.Physics;
using Bus.Common.Rendering;
using Bus.Common.Scenery.Networks;
using Bus.Common.Traffic;
using Bus.Common.Worlds;

namespace Bus.Common.Avatars
{
    public abstract class AvatarBase : RigidBody, ITrafficParticipant
    {
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
        public abstract float Length { get; }
        public abstract bool IsEnabled { get; }
        public abstract LanePath? Path { get; }
        public abstract ParticipantDirection Heading { get; }
        public abstract float S { get; }
        public abstract float SVelocity { get; }
        float ITrafficParticipant.SVelocity => SVelocity;

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
        }

        public abstract bool Spawn(LanePath path, ParticipantDirection heading, float s);
    }
}
