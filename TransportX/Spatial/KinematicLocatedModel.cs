using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;

using TransportX.Physics;
using TransportX.Rendering;

namespace TransportX.Spatial
{
    public class KinematicLocatedModel : CollidableLocatedModel
    {
        public override Pose Pose
        {
            get => base.Pose;
            set => ColliderPose = base.Pose = value;
        }

        internal protected KinematicLocatedModel(IPhysicsHost physicsHost, ICollidableModel model, BodyDescription description, Pose pose)
            : base(physicsHost, model, description, pose)
        {
            Pose = BasePose;
        }


        public static KinematicLocatedModel Create(IPhysicsHost physicsHost, ICollidableModel model, Pose pose)
        {
            RigidPose rigidPose = (model.Collider.Offset * pose).ToRigidPose();
            BodyDescription desc = BodyDescription.CreateKinematic(rigidPose, model.Collider.ShapeIndex, 0.01f);
            return new(physicsHost, model, desc, pose);
        }

        public static LocatedModel CreateKinematicOrNonCollision(IPhysicsHost physicsHost, IModel model, Pose pose)
        {
            return model is ICollidableModel collidableModel
                ? Create(physicsHost, collidableModel, pose) : new LocatedModel(model, pose);
        }

        public override bool SetFromCamera(PlateOffset fromCamera)
        {
            bool isChanged = base.SetFromCamera(fromCamera);
            if (isChanged)
            {
                ColliderPose = Pose;
            }

            return isChanged;
        }

        public DynamicLocatedModel ToDynamic()
        {
            return new(PhysicsHost, Model, Description, BasePose)
            {
                Pose = Pose,
            };
        }
    }
}
