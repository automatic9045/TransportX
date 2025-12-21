using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;

using Bus.Common.Physics;
using Bus.Common.Rendering;
using Bus.Common.Scenery;

namespace Bus.Common.Bodies
{
    public class BodyStructure : IReadOnlyList<LocatedModel>, IDisposable, IDrawable
    {
        protected readonly IPhysicsHost PhysicsHost;
        protected readonly Func<Matrix4x4> TransformFactory;

        protected readonly List<LocatedModel> Items = new List<LocatedModel>();

        public LocatedModel this[int index] => Items[index];
        public int Count => Items.Count;
        public IEnumerator<LocatedModel> GetEnumerator() => Items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public Matrix4x4 Transform => TransformFactory();

        public ColliderGroupHandle DefaultGroup { get; } = ColliderGroupHandle.NewGroup();
        public LocatedModel? RootModel => Count == 0 ? null : this[0];

        public BodyStructure(IPhysicsHost physicsHost, Func<Matrix4x4> transformFactory)
        {
            PhysicsHost = physicsHost;
            TransformFactory = transformFactory;
        }

        private T AttachCollidable<T>(T locatedModel, ColliderGroupHandle group) where T : CollidableLocatedModel
        {
            locatedModel.Transform = locatedModel.BaseTransform * Transform;
            PhysicsHost.SetGroup(locatedModel.Handle, group);

            Items.Add(locatedModel);
            return locatedModel;
        }

        public DynamicLocatedModel AttachDynamic(
            ICollidableModel model, Func<ICollidableModel, RigidPose, BodyDescription> descFactory, ColliderGroupHandle group, Matrix4x4 transform)
        {
            DynamicLocatedModel locatedModel = DynamicLocatedModel.Create(PhysicsHost, model, descFactory, transform);
            return AttachCollidable(locatedModel, group);
        }

        public DynamicLocatedModel AttachDynamic(
            ICollidableModel model, Func<ICollidableModel, RigidPose, BodyDescription> descFactory, ColliderGroupHandle group, SixDoF position)
        {
            return AttachDynamic(model, descFactory, group, position.CreateTransform());
        }

        public DynamicLocatedModel AttachDynamic(ICollidableModel model, float mass, ColliderGroupHandle group, Matrix4x4 transform)
        {
            DynamicLocatedModel locatedModel = DynamicLocatedModel.Create(PhysicsHost, model, mass, transform);
            return AttachCollidable(locatedModel, group);
        }

        public DynamicLocatedModel AttachDynamic(ICollidableModel model, float mass, ColliderGroupHandle group, SixDoF position)
        {
            return AttachDynamic(model, mass, group, position.CreateTransform());
        }

        public DynamicLocatedModel AttachDynamic(ICollidableModel model, float mass, Matrix4x4 transform)
        {
            return AttachDynamic(model, mass, DefaultGroup, transform);
        }

        public DynamicLocatedModel AttachDynamic(ICollidableModel model, float mass, SixDoF position)
        {
            return AttachDynamic(model, mass, DefaultGroup, position.CreateTransform());
        }

        public KinematicLocatedModel AttachKinematic(ICollidableModel model, ColliderGroupHandle group, Matrix4x4 transform)
        {
            KinematicLocatedModel locatedModel = KinematicLocatedModel.Create(PhysicsHost, model, transform);
            return AttachCollidable(locatedModel, group);
        }

        public KinematicLocatedModel AttachKinematic(ICollidableModel model, ColliderGroupHandle group, SixDoF position)
        {
            return AttachKinematic(model, group, position.CreateTransform());
        }

        public KinematicLocatedModel AttachKinematic(ICollidableModel model, Matrix4x4 transform)
        {
            return AttachKinematic(model, DefaultGroup, transform);
        }

        public KinematicLocatedModel AttachKinematic(ICollidableModel model, SixDoF position)
        {
            return AttachKinematic(model, DefaultGroup, position.CreateTransform());
        }

        public LocatedModel Attach(IModel model, Matrix4x4 transform)
        {
            LocatedModel locatedModel = new LocatedModel(model, transform);
            locatedModel.Transform = locatedModel.BaseTransform * Transform;

            Items.Add(locatedModel);
            return locatedModel;
        }

        public LocatedModel Attach(IModel model, SixDoF position)
        {
            return Attach(model, position.CreateTransform());
        }

        public void Dispose()
        {
            foreach (LocatedModel model in Items)
            {
                if (model is CollidableLocatedModel collidableModel) collidableModel.Dispose();
            }
        }

        public void Detach(LocatedModel model)
        {
            Items.Remove(model);
            if (model is CollidableLocatedModel collidableModel) collidableModel.Dispose();
        }

        public void SetFromCamera(PlateOffset fromCamera)
        {
            foreach (LocatedModel model in Items)
            {
                if (model is CollidableLocatedModel collidableModel) collidableModel.SetFromCamera(fromCamera);
            }
        }

        public void Draw(LocatedDrawContext context)
        {
            foreach (LocatedModel model in Items) model.Draw(context);
        }
    }
}
