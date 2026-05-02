using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;

using TransportX.Physics;
using TransportX.Rendering;
using TransportX.Spatial;

namespace TransportX.Bodies
{
    public class BodyStructure : IReadOnlyList<TransformedModel>, IDisposable, IDrawable
    {
        protected readonly IPhysicsHost PhysicsHost;

        protected readonly List<TransformedModel> Items = new List<TransformedModel>();

        public TransformedModel this[int index] => Items[index];
        public int Count => Items.Count;
        public List<TransformedModel>.Enumerator GetEnumerator() => Items.GetEnumerator();
        IEnumerator<TransformedModel> IEnumerable<TransformedModel>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public ColliderGroupHandle DefaultGroup { get; } = ColliderGroupHandle.NewGroup();
        public TransformedModel? RootModel => Count == 0 ? null : this[0];

        public bool IsActive { get; private set; } = true;

        public BodyStructure(IPhysicsHost physicsHost)
        {
            PhysicsHost = physicsHost;
        }

        private T AttachCollidable<T>(T collidable, ColliderGroupHandle group) where T : CollidableTransformedModel
        {
            PhysicsHost.SetGroup(collidable.Handle, group);

            Items.Add(collidable);
            return collidable;
        }

        public DynamicTransformedModel AttachDynamic(
            ICollidableModel model, Func<ICollidableModel, RigidPose, BodyDescription> descFactory, ColliderGroupHandle group, Pose basePose)
        {
            DynamicTransformedModel transformedModel = DynamicTransformedModel.Create(PhysicsHost, model, descFactory, basePose);
            return AttachCollidable(transformedModel, group);
        }

        public DynamicTransformedModel AttachDynamic(
            ICollidableModel model, Func<ICollidableModel, RigidPose, BodyDescription> descFactory, ColliderGroupHandle group, SixDoF basePosition)
        {
            return AttachDynamic(model, descFactory, group, basePosition.ToPose());
        }

        public DynamicTransformedModel AttachDynamic(ICollidableModel model, float mass, ColliderGroupHandle group, Pose basePose)
        {
            DynamicTransformedModel transformedModel = DynamicTransformedModel.Create(PhysicsHost, model, mass, basePose);
            return AttachCollidable(transformedModel, group);
        }

        public DynamicTransformedModel AttachDynamic(ICollidableModel model, float mass, ColliderGroupHandle group, SixDoF basePosition)
        {
            return AttachDynamic(model, mass, group, basePosition.ToPose());
        }

        public DynamicTransformedModel AttachDynamic(ICollidableModel model, float mass, Pose basePose)
        {
            return AttachDynamic(model, mass, DefaultGroup, basePose);
        }

        public DynamicTransformedModel AttachDynamic(ICollidableModel model, float mass, SixDoF basePosition)
        {
            return AttachDynamic(model, mass, DefaultGroup, basePosition.ToPose());
        }

        public KinematicTransformedModel AttachKinematic(ICollidableModel model, ColliderGroupHandle group, Pose pose)
        {
            KinematicTransformedModel transformedModel = KinematicTransformedModel.Create(PhysicsHost, model, pose);
            return AttachCollidable(transformedModel, group);
        }

        public KinematicTransformedModel AttachKinematic(ICollidableModel model, ColliderGroupHandle group, SixDoF position)
        {
            return AttachKinematic(model, group, position.ToPose());
        }

        public KinematicTransformedModel AttachKinematic(ICollidableModel model, Pose pose)
        {
            return AttachKinematic(model, DefaultGroup, pose);
        }

        public KinematicTransformedModel AttachKinematic(ICollidableModel model, SixDoF position)
        {
            return AttachKinematic(model, DefaultGroup, position.ToPose());
        }

        public TransformedModel Attach(IModel model, Pose pose)
        {
            TransformedModel transformedModel = new(model, pose);

            Items.Add(transformedModel);
            return transformedModel;
        }

        public TransformedModel Attach(IModel model, SixDoF position)
        {
            return Attach(model, position.ToPose());
        }

        public TransformedModel AttachKinematicOrNonCollision(IModel model, ColliderGroupHandle group, Pose pose)
        {
            return model is ICollidableModel collidable ? AttachKinematic(collidable, group, pose) : Attach(model, pose);
        }

        public TransformedModel AttachKinematicOrNonCollision(IModel model, ColliderGroupHandle group, SixDoF position)
        {
            return AttachKinematicOrNonCollision(model, group, position.ToPose());
        }

        public TransformedModel AttachKinematicOrNonCollision(IModel model, Pose pose)
        {
            return AttachKinematicOrNonCollision(model, DefaultGroup, pose);
        }

        public TransformedModel AttachKinematicOrNonCollision(IModel model, SixDoF position)
        {
            return AttachKinematicOrNonCollision(model, position.ToPose());
        }

        public void Dispose()
        {
            foreach (TransformedModel model in Items)
            {
                if (model is MergedKinematicTransformedModel mergedModel) mergedModel.Dispose();
            }
        }

        public void Detach(TransformedModel model)
        {
            Items.Remove(model);
            if (model is MergedKinematicTransformedModel mergedModel) mergedModel.Dispose();
        }

        public void SetFromCamera(ChunkOffset fromCamera)
        {
            foreach (TransformedModel model in Items)
            {
                if (model is CollidableTransformedModel collidableModel) collidableModel.SetFromCamera(fromCamera);
            }
        }

        public void Freeze()
        {
            IsActive = false;

            foreach (TransformedModel model in Items)
            {
                if (model is CollidableTransformedModel collidableModel) collidableModel.Freeze();
            }
        }

        public void Unfreeze()
        {
            if (IsActive) return;
            IsActive = true;

            foreach (TransformedModel model in Items)
            {
                if (model is CollidableTransformedModel collidableModel) collidableModel.Unfreeze();
            }
        }

        public void Draw(in TransformedDrawContext context)
        {
            foreach (TransformedModel model in Items) model.Draw(context);
        }
    }
}
