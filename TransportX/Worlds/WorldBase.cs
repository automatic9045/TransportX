using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;

using TransportX.Avatars;
using TransportX.Bodies;
using TransportX.Components;
using TransportX.Dependency;
using TransportX.Diagnostics;
using TransportX.Environment;
using TransportX.Input;
using TransportX.Rendering;
using TransportX.Physics;
using TransportX.Spatial;

namespace TransportX.Worlds
{
    public abstract class WorldBase : IDisposable
    {
        public IWorldInfo Info { get; }
        public IDXHost DXHost { get; }
        public IDXClient DXClient { get; }
        public IPhysicsHost PhysicsHost { get; }
        public IErrorCollector ErrorCollector { get; }
        public PluginLoadContext GameContext { get; }
        public PluginLoadContext WorldContext { get; }
        public TimeManager TimeManager { get; }
        public InputManager InputManager { get; }
        public Camera Camera { get; }

        public string Location { get; protected set; }
        public string BaseDirectory { get; protected set; }

        public abstract IModelCollection Models { get; }

        public EnvironmentProfile DefaultEnvironment { get; protected set; } = EnvironmentProfile.Default;
        public DirectionalLight DirectionalLight { get; protected set; } = DirectionalLight.Default;

        public List<LocatedModel> BackgroundModels { get; } = [];
        public PlateCollection Plates { get; } = [];
        public BodyCollection Bodies { get; } = [];

        private readonly WorldComponentCollection ComponentsKey = [];
        public IComponentCollection<IWorldComponent> Components => ComponentsKey;

        public AvatarBase? Avatar
        {
            get => Camera.Viewpoints.AttachedTo;
            set => Camera.Viewpoints.AttachedTo = value;
        }

        public WorldBase(PluginLoadContext context, WorldBuilder builder)
        {
            Info = builder.Info;
            DXHost = builder.DXHost;
            DXClient = builder.DXClient;
            PhysicsHost = builder.PhysicsHost;
            ErrorCollector = builder.ErrorCollector;
            GameContext = builder.GameContext;
            WorldContext = context;
            TimeManager = builder.TimeManager;
            InputManager = builder.InputManager;
            Camera = builder.Camera;

            Location = builder.Info.Path;
            BaseDirectory = Path.GetDirectoryName(Location)!;
        }

        public virtual void Dispose()
        {
            ComponentsKey.Dispose();

            foreach (LocatedModel model in BackgroundModels) (model as CollidableLocatedModel)?.Dispose();
            Plates.Dispose();
            Bodies.Dispose();

            DefaultEnvironment.Dispose();

            Models.Dispose();
        }

        public virtual void OnStart()
        {
            Validate();
            ComponentsKey.OnStart();
        }

        protected virtual void Validate()
        {
            List<BodyHandle> validHandles = Enumerable.Range(0, PhysicsHost.Simulation.Bodies.HandleToLocation.Length)
                .Select(i => new BodyHandle(i))
                .Where(PhysicsHost.Simulation.Bodies.BodyExists)
                .ToList();

            foreach (Plate plate in Plates)
            {
                RemoveAttachedHandles(plate.Models);
                RemoveAttachedHandles(plate.Network.SelectMany(e => e.Models));
            }
            foreach (RigidBody body in Bodies)
            {
                RemoveAttachedHandles(body.Structure);
            }

            if (validHandles.Count != 0)
            {
                throw new Exception($"正常に管理されていない物理モデルを {validHandles.Count} 個検出しました。これは不正な衝突判定を生じさせる原因となります。");
            }


            void RemoveAttachedHandles(IEnumerable<LocatedModel> models)
            {
                foreach (LocatedModel model in models)
                {
                    if (model is CollidableLocatedModel collidable) validHandles.Remove(collidable.Handle);
                }
            }
        }

        public virtual void SubTick(TimeSpan elapsed)
        {
            ComponentsKey.SubTick(elapsed);
            Bodies.SubTick(elapsed, Camera);
            Camera.UpdateView();
            Plates.SetCameraPosition(Camera);
            Bodies.SetCameraPosition(Camera);
        }

        public virtual void Tick(TimeSpan elapsed)
        {
            ComponentsKey.Tick(elapsed);
            Bodies.Tick(elapsed);
            Plates.SetCameraPosition(Camera);
            Bodies.SetCameraPosition(Camera);
        }

        public virtual AvatarBase CreateAvatar(string path, string? identifier)
        {
            AvatarBuilder builder = new AvatarBuilder()
            {
                DXHost = DXHost,
                DXClient = DXClient,
                PhysicsHost = PhysicsHost,
                ErrorCollector = ErrorCollector,
                GameContext = GameContext,
                WorldContext = WorldContext,
                TimeManager = TimeManager,
                InputManager = InputManager,
                Camera = Camera,
                World = this,
            };

            AvatarBase avatar = builder.Build(path, identifier);
            Bodies.Add(avatar);

            return avatar;
        }

        public virtual void DeleteAvatar(AvatarBase avatar)
        {
            if (Avatar == avatar) Avatar = null;
            if (!Bodies.Remove(avatar))
            {
                throw new InvalidOperationException("このワールドで初期化されたアバターではありません。");
            }

            PluginLoadContext avatarContext = avatar.AvatarContext;
            avatar.Dispose();
            WorldContext.Children.Remove(avatarContext);
            avatarContext.Unload();
        }
    }
}
