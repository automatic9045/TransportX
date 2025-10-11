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
        public IPhysicsHost PhysicsHost { get; }
        public TimeManager TimeManager { get; }
        public InputManager InputManager { get; }
        public Camera Camera { get; }

        public string Location { get; protected set; }
        public string BaseDirectory { get; protected set; }

        public abstract IModelCollection Models { get; }

        public List<LocatedModel> BackgroundModels { get; } = new List<LocatedModel>();
        public PlateCollection Plates { get; } = new PlateCollection();

        public WorldBase(WorldBuilder builder)
        {
            Info = builder.Info;
            DXHost = builder.DXHost;
            PhysicsHost = builder.PhysicsHost;
            TimeManager = builder.TimeManager;
            InputManager = builder.InputManager;
            Camera = builder.Camera;

            Location = builder.Info.Path;
            BaseDirectory = Path.GetDirectoryName(Location)!;
        }

        public virtual void Dispose()
        {
            Models.Dispose();
        }

        public abstract void ComputeTick(TimeSpan elapsed);

        public abstract void Tick(TimeSpan elapsed);
    }
}
