using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using Vortice.Mathematics;

using TransportX.Physics;
using TransportX.Rendering;

namespace TransportX.Spatial
{
    public abstract class BodyTransformedModel : CollidableTransformedModel
    {
        protected readonly BodyDescription Description;

        private Pose FrozenPose = Pose.Identity;
        private BodyVelocity FrozenBodyVelocity = default;

        public bool IsActive { get; private set; } = true;

        public BodyHandle Handle { get; }
        public BodyReference Body => PhysicsHost.Simulation.Bodies[Handle];

        public Vector3 Velocity => Pose.TransformNormal(Body.Velocity.Linear, Model.Collider.OffsetInverse);
        public Vector3 AngularVelocity => Pose.TransformNormal(Body.Velocity.Angular, Model.Collider.OffsetInverse);

        public override Pose Pose
        {
            get => ColliderPose;
            set => ColliderPose = value;
        }

        protected override Pose ColliderRawPose
        {
            get => Body.Pose.ToPose();
            set => Body.Pose = value.ToRigidPose();
        }

        protected override Pose ColliderPose
        {
            set
            {
                PhysicsHost.Simulation.Awakener.AwakenBody(Handle);
                base.ColliderPose = value;
            }
        }

        protected BodyTransformedModel(IPhysicsHost physicsHost, ICollidableModel model, BodyDescription description, Pose basePose)
            : base(physicsHost, model, basePose)
        {
            Description = description;

            Handle = PhysicsHost.Simulation.Bodies.Add(description);
            PhysicsHost.SetMaterial(Handle, Model.Collider.Material);
        }

        public override void Dispose()
        {
            PhysicsHost.Simulation.Bodies.Remove(Handle);
        }

        public override bool SetFromCamera(ChunkIndex fromCamera)
        {
            ChunkIndex delta = fromCamera - FromCamera;

            bool isChanged = base.SetFromCamera(fromCamera);
            if (isChanged)
            {
                PhysicsHost.Simulation.Awakener.AwakenBody(Handle);
                Body.Pose.Position += delta.Position;
            }

            return isChanged;
        }

        public void Freeze()
        {
            if (!IsActive) return;

            FrozenPose = Pose;
            FrozenBodyVelocity = Body.Velocity;

            Body.Velocity = default;
            IsActive = false;
        }

        public void Unfreeze()
        {
            if (IsActive) return;

            IsActive = true;
            Pose = FrozenPose;
            Body.Velocity = FrozenBodyVelocity;
        }
    }
}
