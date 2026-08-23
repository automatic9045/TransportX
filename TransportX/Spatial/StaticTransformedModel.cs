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

        protected StaticTransformedModel(IPhysicsHost physicsHost, in ModelResourceSet resource, StaticDescription description, Pose pose)
            : base(resource, pose)
        {
            PhysicsHost = physicsHost;
            Description = description;
            Handle = PhysicsHost.Simulation.Statics.Add(description);
            PhysicsHost.SetMaterial(Handle, Collider.Material);

            Pose = BasePose;
        }

        public static StaticTransformedModel Create(IPhysicsHost physicsHost, in ModelResourceSet resource, Pose pose)
        {
            RigidPose rigidPose = (resource.GetCollider().Offset * pose).ToRigidPose();
            StaticDescription desc = new(rigidPose, resource.Collider.ShapeIndex);
            return new StaticTransformedModel(physicsHost, resource, desc, pose);
        }

        public static TransformedModel CreateStaticOrNonCollision(IPhysicsHost physicsHost, in ModelResourceSet resource, Pose pose)
        {
            return resource.Collider is null
                ? new TransformedModel(resource, pose) : Create(physicsHost, resource, pose);
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
