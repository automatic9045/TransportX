using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;

using Bus.Common.Physics;
using Bus.Common.Rendering;

namespace Bus.Common.Scenery
{
    public class KinematicLocatedModel : CollidableLocatedModel
    {
        public override Pose Pose
        {
            get => base.Pose;
            set => ColliderPose = base.Pose = value;
        }

        internal protected KinematicLocatedModel(Simulation simulation, ICollidableModel model, BodyHandle handle, Pose pose)
            : base(simulation, model, handle, pose)
        {
            Pose = BasePose;
        }


        public static KinematicLocatedModel Create(IPhysicsHost physicsHost, ICollidableModel model, Pose pose)
        {
            RigidPose rigidPose = (model.Collider.Offset * pose).ToRigidPose();
            BodyDescription desc = BodyDescription.CreateKinematic(rigidPose, model.Collider.ShapeIndex, 0.01f);

            BodyHandle handle = physicsHost.Simulation.Bodies.Add(desc);
            physicsHost.SetMaterial(handle, model.Collider.Material);

            return new(physicsHost.Simulation, model, handle, pose);
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
            return new(Simulation, Model, Handle, BasePose)
            {
                Pose = Pose,
            };
        }
    }
}
