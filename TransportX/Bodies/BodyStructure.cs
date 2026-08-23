using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using Vortice.Mathematics;

using TransportX.Physics;
using TransportX.Rendering;
using TransportX.Spatial;

namespace TransportX.Bodies
{
    public class BodyStructure : IReadOnlyList<TransformedModel>, IDisposable
    {
        protected readonly IPhysicsHost PhysicsHost;

        protected readonly List<TransformedModel> Items = [];

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

        private T AttachCollidable<T>(T collidable, ColliderGroupHandle group) where T : BodyTransformedModel
        {
            PhysicsHost.SetGroup(collidable.Handle, group);

            Items.Add(collidable);
            return collidable;
        }

        public DynamicTransformedModel AttachDynamic(
            in ModelResourceSet model, Func<ModelResourceSet, RigidPose, BodyDescription> descFactory, ColliderGroupHandle group, Pose basePose)
        {
            DynamicTransformedModel transformedModel = DynamicTransformedModel.Create(PhysicsHost, model, descFactory, basePose);
            return AttachCollidable(transformedModel, group);
        }

        public DynamicTransformedModel AttachDynamic(in ModelResourceSet model, float mass, ColliderGroupHandle group, Pose basePose)
        {
            DynamicTransformedModel transformedModel = DynamicTransformedModel.Create(PhysicsHost, model, mass, basePose);
            return AttachCollidable(transformedModel, group);
        }

        public DynamicTransformedModel AttachDynamic(in ModelResourceSet model, float mass, Pose basePose)
        {
            return AttachDynamic(model, mass, DefaultGroup, basePose);
        }

        public KinematicTransformedModel AttachKinematic(in ModelResourceSet model, ColliderGroupHandle group, Pose pose)
        {
            KinematicTransformedModel transformedModel = KinematicTransformedModel.Create(PhysicsHost, model, pose);
            return AttachCollidable(transformedModel, group);
        }

        public KinematicTransformedModel AttachKinematic(in ModelResourceSet model, Pose pose)
        {
            return AttachKinematic(model, DefaultGroup, pose);
        }

        public TransformedModel Attach(in ModelResourceSet model, Pose pose)
        {
            TransformedModel transformedModel = new(model, pose);

            Items.Add(transformedModel);
            return transformedModel;
        }

        public TransformedModel AttachKinematicOrNonCollision(in ModelResourceSet model, ColliderGroupHandle group, Pose pose)
        {
            return model.Collider is null ? Attach(model, pose) : AttachKinematic(model, group, pose);
        }

        public TransformedModel AttachKinematicOrNonCollision(in ModelResourceSet model, Pose pose)
        {
            return AttachKinematicOrNonCollision(model, DefaultGroup, pose);
        }

        public void Dispose()
        {
            foreach (TransformedModel model in Items)
            {
                if (model is CollidableTransformedModel collidableModel) collidableModel.Dispose();
            }
        }

        public void Detach(TransformedModel model)
        {
            Items.Remove(model);
            if (model is CollidableTransformedModel collidableModel) collidableModel.Dispose();
        }

        public void SetFromCamera(ChunkIndex fromCamera)
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
                if (model is BodyTransformedModel bodyModel) bodyModel.Freeze();
            }
        }

        public void Unfreeze()
        {
            if (IsActive) return;
            IsActive = true;

            foreach (TransformedModel model in Items)
            {
                if (model is BodyTransformedModel bodyModel) bodyModel.Unfreeze();
            }
        }

        public void Draw<TCuller>(in TransformedDrawContext context, in TCuller culler) where TCuller : struct, ICullingVolume
        {
            foreach (TransformedModel model in Items)
            {
                Matrix4x4 world = (model.Pose * context.ChunkOffset.Pose).ToMatrix4x4();
                BoundingBox worldBox = BoundingBox.Transform(model.Resource.Model.BoundingBox, world);

                if (culler.Intersects(worldBox))
                {
                    model.Draw(context);
                }
            }
        }
    }
}
