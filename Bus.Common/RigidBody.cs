using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;

using Bus.Common.Rendering;
using Bus.Common.Scenery;

namespace Bus.Common
{
    public class RigidBody : LocatableObject, IDisposable, IDrawable
    {
        private readonly Simulation Simulation;

        private readonly List<DynamicLocatedModel> ModelsKey = new List<DynamicLocatedModel>();
        public IReadOnlyList<DynamicLocatedModel> Models => ModelsKey;

        public RigidBody(Simulation simulation, int plateX, int plateZ, Matrix4x4 locator) : base(plateX, plateZ, locator)
        {
            Simulation = simulation;
        }

        public RigidBody(Simulation simulation, int plateX, int plateZ, SixDoF position) : this(simulation, plateX, plateZ, position.CreateTransform())
        {
        }

        public RigidBody(Simulation simulation) : this(simulation, 0, 0, Matrix4x4.Identity)
        {
        }

        public virtual void Dispose()
        {
        }

        public DynamicLocatedModel AttachModel(ICollidableModel model, float mass, Matrix4x4 locator)
        {
            DynamicLocatedModel locatedModel = LocatedModel.CreateDynamic(Simulation, model, mass, locator);
            locatedModel.Locator = locatedModel.InitialLocator * Locator;

            ModelsKey.Add(locatedModel);
            return locatedModel;
        }

        public virtual void ComputeTick(TimeSpan elapsed)
        {
            if (Models.Count == 0) return;

            foreach (DynamicLocatedModel model in Models) model.SyncLocator();

            PlateOffset plateOffset = Locate(PlateX, PlateZ, Models[0].InitialLocatorInverse * Models[0].Locator);
            if (!plateOffset.IsZero)
            {
                foreach (DynamicLocatedModel model in Models) model.Locator = model.InitialLocator * Locator;
            }
        }

        public virtual void Tick(TimeSpan elapsed)
        {
        }

        public virtual void Draw(DrawContext context)
        {
            foreach (DynamicLocatedModel model in Models) model.Draw(context);
        }
    }
}
