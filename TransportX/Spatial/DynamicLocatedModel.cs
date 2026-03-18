using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;

using TransportX.Physics;
using TransportX.Rendering;

namespace TransportX.Spatial
{
    public class DynamicLocatedModel : CollidableLocatedModel
    {
        public override Pose Pose
        {
            get => ColliderPose;
            set => ColliderPose = value;
        }

        internal protected DynamicLocatedModel(IPhysicsHost physicsHost, ICollidableModel model, BodyDescription description, Pose basePose)
            : base(physicsHost, model, description, basePose)
        {
        }

        public static DynamicLocatedModel Create(IPhysicsHost physicsHost,
            ICollidableModel model, Func<ICollidableModel, RigidPose, BodyDescription> descFactory, Pose basePose)
        {
            BodyDescription desc = descFactory(model, (model.Collider.Offset * basePose).ToRigidPose());
            return new DynamicLocatedModel(physicsHost, model, desc, basePose);
        }

        public static DynamicLocatedModel Create(IPhysicsHost physicsHost,
            ICollidableModel model, float mass, CollidableDescription collidableDescription, Pose basePose)
        {
            BodyInertia inertia = model.Collider.ComputeInertia(mass);
            BodyDescription CreateDesc(ICollidableModel model, RigidPose pose)
                => BodyDescription.CreateDynamic(pose, inertia, collidableDescription, 0.01f);

            return Create(physicsHost, model, CreateDesc, basePose);
        }

        public static DynamicLocatedModel Create(IPhysicsHost physicsHost, ICollidableModel model, float mass, Pose basePose)
        {
            return Create(physicsHost, model, mass, model.Collider.ShapeIndex, basePose);
        }

        public override bool SetFromCamera(PlateOffset fromCamera)
        {
            PlateOffset delta = fromCamera - FromCamera;

            bool isChanged = base.SetFromCamera(fromCamera);
            if (isChanged)
            {
                PhysicsHost.Simulation.Awakener.AwakenBody(Handle);
                Body.Pose.Position += delta.Position;
            }

            return isChanged;
        }

        public void Shift(PlateOffset offset)
        {
            Pose *= offset.PoseInverse;
        }
    }
}
