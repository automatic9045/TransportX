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

namespace Bus.Common.Scenery
{
    public class LocatedModelCollection : IReadOnlyList<LocatedModel>, IDrawable
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
        public DynamicLocatedModel? RootModel => Count == 0 ? null : (DynamicLocatedModel)this[0];

        public LocatedModelCollection(IPhysicsHost physicsHost, Func<Matrix4x4> transformFactory)
        {
            PhysicsHost = physicsHost;
            TransformFactory = transformFactory;
        }

        private DynamicLocatedModel Attach(DynamicLocatedModel locatedModel, ColliderGroupHandle group)
        {
            locatedModel.Transform = locatedModel.BaseTransform * Transform;
            PhysicsHost.AddToGroup(locatedModel.Handle, group);

            Items.Add(locatedModel);
            return locatedModel;
        }

        public DynamicLocatedModel Attach(
            ICollidableModel model, Func<ICollidableModel, RigidPose, BodyDescription> descFactory, ColliderGroupHandle group, Matrix4x4 transform)
        {
            DynamicLocatedModel locatedModel = DynamicLocatedModel.Create(PhysicsHost.Simulation, model, descFactory, transform);
            return Attach(locatedModel, group);
        }

        public DynamicLocatedModel Attach(
            ICollidableModel model, Func<ICollidableModel, RigidPose, BodyDescription> descFactory, ColliderGroupHandle group, SixDoF position)
        {
            return Attach(model, descFactory, group, position.CreateTransform());
        }

        public DynamicLocatedModel Attach(ICollidableModel model, float mass, ColliderGroupHandle group, Matrix4x4 transform)
        {
            DynamicLocatedModel locatedModel = DynamicLocatedModel.Create(PhysicsHost.Simulation, model, mass, transform);
            return Attach(locatedModel, group);
        }

        public DynamicLocatedModel Attach(ICollidableModel model, float mass, ColliderGroupHandle group, SixDoF position)
        {
            return Attach(model, mass, group, position.CreateTransform());
        }

        public DynamicLocatedModel Attach(ICollidableModel model, float mass, Matrix4x4 transform)
        {
            return Attach(model, mass, DefaultGroup, transform);
        }

        public DynamicLocatedModel Attach(ICollidableModel model, float mass, SixDoF position)
        {
            return Attach(model, mass, DefaultGroup, position.CreateTransform());
        }

        public LocatedModel Attach(IModel model, Matrix4x4 transform)
        {
            if (RootModel is null) throw new InvalidOperationException("1 つ目のモデル (ルートモデル) は剛体である必要があります。");

            LocatedModel locatedModel = new LocatedModel(model, transform);
            locatedModel.Transform = locatedModel.BaseTransform * Transform;

            Items.Add(locatedModel);
            return locatedModel;
        }

        public LocatedModel Attach(IModel model, SixDoF position)
        {
            return Attach(model, position.CreateTransform());
        }

        public void Draw(DrawContext context)
        {
            foreach (LocatedModel model in this) model.Draw(context);
        }
    }
}
