using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Input;
using Bus.Common.Rendering;
using Bus.Common.Physics;
using Bus.Common.Scenery;

namespace Bus.Common.Worlds
{
    public abstract class WorldBase : IDisposable
    {
        public IWorldInfo Info { get; }
        public IDXHost DXHost { get; }
        public IDXClient DXClient { get; }
        public IPhysicsHost PhysicsHost { get; }
        public TimeManager TimeManager { get; }
        public InputManager InputManager { get; }
        public Camera Camera { get; }

        public string Location { get; protected set; }
        public string BaseDirectory { get; protected set; }

        public abstract IModelCollection Models { get; }

        public List<LocatedModel> BackgroundModels { get; } = new List<LocatedModel>();
        public PlateCollection Plates { get; } = new PlateCollection();
        public List<RigidBody> Bodies { get; } = new List<RigidBody>();

        public WorldBase(WorldBuilder builder)
        {
            Info = builder.Info;
            DXHost = builder.DXHost;
            DXClient = builder.DXClient;
            PhysicsHost = builder.PhysicsHost;
            TimeManager = builder.TimeManager;
            InputManager = builder.InputManager;
            Camera = builder.Camera;

            Location = builder.Info.Path;
            BaseDirectory = Path.GetDirectoryName(Location)!;
        }

        public virtual void Dispose()
        {
            foreach (RigidBody body in Bodies) body.Dispose();
            Models.Dispose();
        }

        public virtual void SubTick(TimeSpan elapsed)
        {
            Plates.Update(Camera.PlateX, Camera.PlateZ);
            foreach (RigidBody body in Bodies) body.SubTick(elapsed);
        }

        public virtual void Tick(TimeSpan elapsed)
        {
            foreach (RigidBody body in Bodies) body.Tick(elapsed);
        }
    }
}
