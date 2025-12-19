using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;

using Bus.Common.Bodies;
using Bus.Common.Dependency;
using Bus.Common.Diagnostics;
using Bus.Common.Input;
using Bus.Common.Rendering;
using Bus.Common.Physics;
using Bus.Common.Scenery;
using Bus.Common.Vehicles;

namespace Bus.Common.Worlds
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

        public List<LocatedModel> BackgroundModels { get; } = new List<LocatedModel>();
        public PlateCollection Plates { get; } = new PlateCollection();
        public BodyCollection Bodies { get; } = new BodyCollection();

        public VehicleBase? UserVehicle
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
            Bodies.Dispose();
            Models.Dispose();
        }

        public virtual void OnStart()
        {
            Validate();
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
                RemoveAttachedHandles(body.Models);
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

        public virtual void SetCameraPosition()
        {
            Plates.SetCameraPosition(Camera);
            Bodies.SetCameraPosition(Camera);
        }

        public virtual void SubTick(TimeSpan elapsed)
        {
            Bodies.SubTick(elapsed, Camera);
        }

        public virtual void Tick(TimeSpan elapsed)
        {
            Bodies.Tick(elapsed);
        }

        public virtual VehicleBase CreateVehicle(string path, string? identifier)
        {
            VehicleBuilder vehicleBuilder = new VehicleBuilder()
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

            VehicleBase vehicle = vehicleBuilder.Build(path, identifier);
            Bodies.Add(vehicle);

            return vehicle;
        }

        public virtual void DeleteVehicle(VehicleBase vehicle)
        {
            if (UserVehicle == vehicle) UserVehicle = null;
            if (!Bodies.Remove(vehicle))
            {
                throw new InvalidOperationException("このワールドで初期化された車両ではありません。");
            }

            PluginLoadContext vehicleContext = vehicle.VehicleContext;
            vehicle.Dispose();
            WorldContext.Children.Remove(vehicleContext);
            vehicleContext.Unload();
        }
    }
}
