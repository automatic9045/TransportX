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

        protected readonly List<LocatedModel> Items = new List<LocatedModel>();

        public LocatedModel this[int index] => Items[index];
        public int Count => Items.Count;
        public IEnumerator<LocatedModel> GetEnumerator() => Items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public ColliderGroupHandle DefaultGroup { get; } = ColliderGroupHandle.NewGroup();
        public LocatedModel? RootModel => Count == 0 ? null : this[0];

        public BodyStructure(IPhysicsHost physicsHost)
        {
            PhysicsHost = physicsHost;
        }

        private T AttachCollidable<T>(T locatedModel, ColliderGroupHandle group) where T : CollidableLocatedModel
        {
            PhysicsHost.SetGroup(locatedModel.Handle, group);

            Items.Add(locatedModel);
            return locatedModel;
        }

        public DynamicLocatedModel AttachDynamic(
            ICollidableModel model, Func<ICollidableModel, RigidPose, BodyDescription> descFactory, ColliderGroupHandle group, Pose basePose)
        {
            DynamicLocatedModel locatedModel = DynamicLocatedModel.Create(PhysicsHost, model, descFactory, basePose);
            return AttachCollidable(locatedModel, group);
        }

        public DynamicLocatedModel AttachDynamic(
            ICollidableModel model, Func<ICollidableModel, RigidPose, BodyDescription> descFactory, ColliderGroupHandle group, SixDoF basePosition)
        {
            return AttachDynamic(model, descFactory, group, basePosition.ToPose());
        }

        public DynamicLocatedModel AttachDynamic(ICollidableModel model, float mass, ColliderGroupHandle group, Pose basePose)
        {
            DynamicLocatedModel locatedModel = DynamicLocatedModel.Create(PhysicsHost, model, mass, basePose);
            return AttachCollidable(locatedModel, group);
        }

        public DynamicLocatedModel AttachDynamic(ICollidableModel model, float mass, ColliderGroupHandle group, SixDoF basePosition)
        {
            return AttachDynamic(model, mass, group, basePosition.ToPose());
        }

        public DynamicLocatedModel AttachDynamic(ICollidableModel model, float mass, Pose basePose)
        {
            return AttachDynamic(model, mass, DefaultGroup, basePose);
        }

        public DynamicLocatedModel AttachDynamic(ICollidableModel model, float mass, SixDoF basePosition)
        {
            return AttachDynamic(model, mass, DefaultGroup, basePosition.ToPose());
        }

        public KinematicLocatedModel AttachKinematic(ICollidableModel model, ColliderGroupHandle group, Pose pose)
        {
            KinematicLocatedModel locatedModel = KinematicLocatedModel.Create(PhysicsHost, model, pose);
            return AttachCollidable(locatedModel, group);
        }

        public KinematicLocatedModel AttachKinematic(ICollidableModel model, ColliderGroupHandle group, SixDoF position)
        {
            return AttachKinematic(model, group, position.ToPose());
        }

        public KinematicLocatedModel AttachKinematic(ICollidableModel model, Pose pose)
        {
            return AttachKinematic(model, DefaultGroup, pose);
        }

        public KinematicLocatedModel AttachKinematic(ICollidableModel model, SixDoF position)
        {
            return AttachKinematic(model, DefaultGroup, position.ToPose());
        }

        public LocatedModel Attach(IModel model, Pose pose)
        {
            LocatedModel locatedModel = new LocatedModel(model, pose);

            Items.Add(locatedModel);
            return locatedModel;
        }

        public LocatedModel Attach(IModel model, SixDoF position)
        {
            return Attach(model, position.ToPose());
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
