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
    public class KinematicTransformedModel : BodyTransformedModel
    {
        protected KinematicTransformedModel(IPhysicsHost physicsHost, in ModelResourceSet resource, BodyDescription description, Pose pose)
            : base(physicsHost, resource, description, pose)
        {
            Pose = BasePose;
        }


        public static KinematicTransformedModel Create(IPhysicsHost physicsHost, in ModelResourceSet resource, Pose pose)
        {
            RigidPose rigidPose = (resource.GetCollider().Offset * pose).ToRigidPose();
            BodyDescription desc = BodyDescription.CreateKinematic(rigidPose, resource.GetCollider().ShapeIndex, 0.01f);
            return new KinematicTransformedModel(physicsHost, resource, desc, pose);
        }

        public static TransformedModel CreateKinematicOrNonCollision(IPhysicsHost physicsHost, in ModelResourceSet resource, Pose pose)
        {
            return resource.Collider is null
                ? new TransformedModel(resource, pose) : Create(physicsHost, resource, pose);
        }
    }
}
