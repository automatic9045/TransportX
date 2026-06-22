using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;

using TransportX.Avatars;
using TransportX.Bodies;
using TransportX.Cameras;
using TransportX.Components;
using TransportX.Dependency;
using TransportX.Diagnostics;
using TransportX.Environment;
using TransportX.Input;
using TransportX.Rendering.Backend;
using TransportX.Physics;
using TransportX.Spatial;

namespace TransportX.Worlds
{
    public abstract class WorldBase : IDisposable
    {
        public IWorldInfo Info { get; }
        public Platform Platform { get; }
        public IDXHost DXHost { get; }
        public IDXClient DXClient { get; }
        public IPhysicsHost PhysicsHost { get; }
        public WorldOptions Options { get; }
        public IErrorCollector ErrorCollector { get; }
        public PluginLoadContext AppContext { get; }
        public PluginLoadContext WorldContext { get; }
        public TimeManager TimeManager { get; }
        public InputManager InputManager { get; }
        public Camera Camera { get; }

        public string Location { get; protected set; }
        public string BaseDirectory { get; protected set; }

        public abstract IModelCollection Models { get; }

        public EnvironmentProfile DefaultEnvironment { get; protected set; } = EnvironmentProfile.Default;
        public DirectionalLight DirectionalLight { get; protected set; } = DirectionalLight.Default;

        public List<TransformedModel> BackgroundModels { get; } = [];
        public ChunkCollection Chunks { get; } = [];
        public BodyCollection Bodies { get; } = [];

        public IComponentCollection<IComponent> Components { get; } = new ComponentCollection<IComponent>();
        public ComponentEngine ComponentEngine { get; } = new();

        public AvatarBase? Avatar
        {
            get => Camera.Viewpoints.AttachedTo;
            set => Camera.Viewpoints.AttachedTo = value;
        }

        public WorldBase(PluginLoadContext context, WorldBuilder builder)
        {
            Info = builder.Info;
            Platform = builder.Platform;
            DXHost = builder.DXHost;
            DXClient = builder.DXClient;
            PhysicsHost = builder.PhysicsHost;
            Options = builder.Options;
            ErrorCollector = builder.ErrorCollector;
            AppContext = builder.AppContext;
            WorldContext = context;
            TimeManager = builder.TimeManager;
            InputManager = builder.InputManager;
            Camera = builder.Camera;

            Location = builder.Info.Path;
            BaseDirectory = Path.GetDirectoryName(Location)!;

            ComponentEngine.Register(Components);
        }

        public virtual void Dispose()
        {
            ComponentEngine.Dispose();

            Chunks.Dispose();
            Bodies.Dispose();

            Models.Dispose();
        }

        public virtual void OnStart()
        {
            Validate();
            Chunks.RegisterComponents(ComponentEngine);
            ComponentEngine.OnStart();
        }

        protected virtual void Validate()
        {
            List<StaticHandle> validStaticHandles = Enumerable.Range(0, PhysicsHost.Simulation.Statics.HandleToIndex.Length)
                .Select(i => new StaticHandle(i))
                .Where(PhysicsHost.Simulation.Statics.StaticExists)
                .ToList();

            List<BodyHandle> validBodyHandles = Enumerable.Range(0, PhysicsHost.Simulation.Bodies.HandleToLocation.Length)
                .Select(i => new BodyHandle(i))
                .Where(PhysicsHost.Simulation.Bodies.BodyExists)
                .ToList();

            foreach (Chunk chunk in Chunks)
            {
                RemoveAttachedHandles(chunk.Models);
                RemoveAttachedHandles(chunk.Network.SelectMany(e => e.Models));
            }
            foreach (RigidBody body in Bodies)
            {
                RemoveAttachedHandles(body.Structure);
            }

            if (validStaticHandles.Count != 0)
            {
                throw new Exception($"正常に管理されていない物理モデル (静的) を {validStaticHandles.Count} 個検出しました。これは不正な衝突判定を生じさせる原因となります。");
            }

            if (validBodyHandles.Count != 0)
            {
                throw new Exception($"正常に管理されていない物理モデル (剛体) を {validBodyHandles.Count} 個検出しました。これは不正な衝突判定を生じさせる原因となります。");
            }


            void RemoveAttachedHandles(IEnumerable<TransformedModel> models)
            {
                foreach (TransformedModel model in models)
                {
                    switch (model)
                    {
                        case BodyTransformedModel bodyModel:
                            validBodyHandles.Remove(bodyModel.Handle);
                            break;

                        case StaticTransformedModel staticModel:
                            validStaticHandles.Remove(staticModel.Handle);
                            break;
                    }
                }
            }
        }

        public virtual void SubTick(TimeSpan elapsed)
        {
            ComponentEngine.SubTick(elapsed);
            Bodies.SubTick(elapsed, Camera.WorldPose, Options.SimulationChunkCount);
            Camera.UpdateView();
            Chunks.SetCameraPosition(Camera.WorldPose, Options.SimulationChunkCount);
            Bodies.SetCameraPosition(Camera.WorldPose, Options.SimulationChunkCount);
        }

        public virtual void Tick(TimeSpan elapsed)
        {
            ComponentEngine.Tick(elapsed, TimeManager.Now);
            Bodies.Tick(elapsed);
            Chunks.SetCameraPosition(Camera.WorldPose, Options.SimulationChunkCount);
            Bodies.SetCameraPosition(Camera.WorldPose, Options.SimulationChunkCount);
        }

        public virtual AvatarBase CreateAvatar(string path, string? identifier)
        {
            AvatarBuilder builder = new()
            {
                Platform = Platform,
                DXHost = DXHost,
                DXClient = DXClient,
                PhysicsHost = PhysicsHost,
                ErrorCollector = ErrorCollector,
                AppContext = AppContext,
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
