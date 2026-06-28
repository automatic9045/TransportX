using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;

using TransportX.Physics;
using TransportX.Rendering;

namespace TransportX.Spatial
{
    public class StaticTransformedModel : CollidableTransformedModel
    {
        protected readonly IPhysicsHost PhysicsHost;
        protected readonly StaticDescription Description;

        public StaticHandle Handle { get; }
        public StaticReference Static => PhysicsHost.Simulation.Statics[Handle];

        public override Pose Pose
        {
            get => base.Pose;
            set => ColliderPose = base.Pose = value;
        }

        protected override Pose ColliderRawPose
        {
            get => Static.Pose.ToPose();
            set
            {
                PhysicsHost.Simulation.Statics.ApplyDescription(Handle, Description with
                {
                    Pose = value.ToRigidPose(),
                });
            }
        }

        internal protected StaticTransformedModel(IPhysicsHost physicsHost, ICollidableModel model, StaticDescription description, Pose pose)
            : base(model, pose)
        {
            PhysicsHost = physicsHost;
            Description = description;
            Handle = PhysicsHost.Simulation.Statics.Add(description);
            PhysicsHost.SetMaterial(Handle, Model.Collider.Material);

            Pose = BasePose;
        }


        public static StaticTransformedModel Create(IPhysicsHost physicsHost, ICollidableModel model, Pose pose)
        {
            RigidPose rigidPose = (model.Collider.Offset * pose).ToRigidPose();
            StaticDescription desc = new(rigidPose, model.Collider.ShapeIndex);
            return new StaticTransformedModel(physicsHost, model, desc, pose);
        }

        public static TransformedModel CreateStaticOrNonCollision(IPhysicsHost physicsHost, IModel model, Pose pose)
        {
            return model is ICollidableModel collidableModel
                ? Create(physicsHost, collidableModel, pose) : new TransformedModel(model, pose);
        }

        public override void Dispose()
        {
            PhysicsHost.Simulation.Statics.Remove(Handle);
        }

        public override bool SetFromCamera(ChunkIndex fromCamera)
        {
            bool isChanged = base.SetFromCamera(fromCamera);
            if (isChanged)
            {
                ColliderPose = Pose;
            }

            return isChanged;
        }
    }
}
