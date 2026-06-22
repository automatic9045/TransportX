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
        internal protected KinematicTransformedModel(IPhysicsHost physicsHost, ICollidableModel model, BodyDescription description, Pose pose)
            : base(physicsHost, model, description, pose)
        {
            Pose = BasePose;
        }


        public static KinematicTransformedModel Create(IPhysicsHost physicsHost, ICollidableModel model, Pose pose)
        {
            RigidPose rigidPose = (model.Collider.Offset * pose).ToRigidPose();
            BodyDescription desc = BodyDescription.CreateKinematic(rigidPose, model.Collider.ShapeIndex, 0.01f);
            return new KinematicTransformedModel(physicsHost, model, desc, pose);
        }

        public static TransformedModel CreateKinematicOrNonCollision(IPhysicsHost physicsHost, IModel model, Pose pose)
        {
            return model is ICollidableModel collidableModel
                ? Create(physicsHost, collidableModel, pose) : new TransformedModel(model, pose);
        }
    }
}
