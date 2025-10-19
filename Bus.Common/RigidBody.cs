using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;

using Bus.Common.Physics;
using Bus.Common.Rendering;
using Bus.Common.Scenery;

namespace Bus.Common
{
    public class RigidBody : LocatableObject, IDisposable, IDrawable
    {
        private readonly IPhysicsHost PhysicsHost;
        private readonly LocatableObject Camera;

        public ColliderGroupHandle DefaultGroup { get; } = ColliderGroupHandle.NewGroup();

        private readonly List<LocatedModel> ModelsKey = new List<LocatedModel>();
        public IReadOnlyList<LocatedModel> Models => ModelsKey;
        public DynamicLocatedModel? RootModel => Models.Count == 0 ? null : (DynamicLocatedModel)Models[0];

        public RigidBody(IPhysicsHost physicsHost, LocatableObject camera, int plateX, int plateZ, Matrix4x4 locator) : base(plateX, plateZ, locator)
        {
            PhysicsHost = physicsHost;
            Camera = camera;
        }

        public RigidBody(IPhysicsHost physicsHost, LocatableObject camera, int plateX, int plateZ, SixDoF position)
            : this(physicsHost, camera, plateX, plateZ, position.CreateTransform())
        {
        }

        public RigidBody(IPhysicsHost physicsHost, LocatableObject camera) : this(physicsHost, camera, 0, 0, Matrix4x4.Identity)
        {
        }

        public virtual void Dispose()
        {
        }

        private DynamicLocatedModel AttachModel(DynamicLocatedModel locatedModel, ColliderGroupHandle group)
        {
            locatedModel.Locator = locatedModel.InitialLocator * Locator;
            PhysicsHost.AddToGroup(locatedModel.Handle, group);

            ModelsKey.Add(locatedModel);
            return locatedModel;
        }

        public DynamicLocatedModel AttachModel(
            ICollidableModel model, Func<ICollidableModel, RigidPose, BodyDescription> descFactory, ColliderGroupHandle group, Matrix4x4 locator)
        {
            DynamicLocatedModel locatedModel = DynamicLocatedModel.Create(PhysicsHost.Simulation, model, descFactory, locator);
            return AttachModel(locatedModel, group);
        }

        public DynamicLocatedModel AttachModel(
            ICollidableModel model, Func<ICollidableModel, RigidPose, BodyDescription> descFactory, ColliderGroupHandle group, SixDoF position)
        {
            return AttachModel(model, descFactory, group, position.CreateTransform());
        }

        public DynamicLocatedModel AttachModel(ICollidableModel model, float mass, ColliderGroupHandle group, Matrix4x4 locator)
        {
            DynamicLocatedModel locatedModel = DynamicLocatedModel.Create(PhysicsHost.Simulation, model, mass, locator);
            return AttachModel(locatedModel, group);
        }

        public DynamicLocatedModel AttachModel(ICollidableModel model, float mass, ColliderGroupHandle group, SixDoF position)
        {
            return AttachModel(model, mass, group, position.CreateTransform());
        }

        public DynamicLocatedModel AttachModel(ICollidableModel model, float mass, Matrix4x4 locator)
        {
            return AttachModel(model, mass, DefaultGroup, locator);
        }

        public DynamicLocatedModel AttachModel(ICollidableModel model, float mass, SixDoF position)
        {
            return AttachModel(model, mass, DefaultGroup, position.CreateTransform());
        }

        public LocatedModel AttachModel(IModel model, Matrix4x4 locator)
        {
            if (RootModel is null) throw new InvalidOperationException("1 つ目のモデル (ルートモデル) は剛体である必要があります。");

            LocatedModel locatedModel = new LocatedModel(model, locator);
            locatedModel.Locator = locatedModel.InitialLocator * Locator;

            ModelsKey.Add(locatedModel);
            return locatedModel;
        }

        public LocatedModel AttachModel(IModel model, SixDoF position)
        {
            return AttachModel(model, position.CreateTransform());
        }

        public virtual void ComputeTick(TimeSpan elapsed)
        {
            if (RootModel is null) return;

            PlateOffset fromCamera = Camera.GetPlateOffset(this);
            foreach (LocatedModel model in Models)
            {
                if (model is CollidableLocatedModel dynamicModel) dynamicModel.ComputeTick(fromCamera);
            }

            PlateOffset plateOffset = Locate(PlateX, PlateZ, RootModel!.InitialLocatorInverse * RootModel.Locator);
            if (!plateOffset.IsZero)
            {
                foreach (LocatedModel model in Models)
                {
                    if (model is DynamicLocatedModel dynamicModel)
                    {
                        dynamicModel.Shift(plateOffset);
                    }
                    else
                    {
                        model.Locator = model.InitialLocator * Locator;
                    }
                }
            }
        }

        public virtual void Tick(TimeSpan elapsed)
        {
        }

        public virtual void Draw(DrawContext context)
        {
            foreach (LocatedModel model in Models) model.Draw(context);
        }
    }
}
