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
        public override Matrix4x4 Transform
        {
            get => base.Transform;
            set => ColliderTransform = base.Transform = value;
        }

        internal protected KinematicLocatedModel(Simulation simulation, ICollidableModel model, BodyHandle handle, Matrix4x4 transform)
            : base(simulation, model, handle, transform)
        {
            Transform = BaseTransform;
        }


        public static KinematicLocatedModel Create(IPhysicsHost physicsHost, ICollidableModel model, Matrix4x4 transform)
        {
            RigidPose pose = (model.Collider.Offset * transform).ToRigidPose();
            BodyDescription desc = BodyDescription.CreateKinematic(pose, model.Collider.ShapeIndex, 0.01f);

            BodyHandle handle = physicsHost.Simulation.Bodies.Add(desc);
            physicsHost.SetMaterial(handle, model.Collider.Material);

            return new(physicsHost.Simulation, model, handle, transform);
        }

        public static LocatedModel CreateKinematicOrNonCollision(IPhysicsHost physicsHost, IModel model, Matrix4x4 transform)
        {
            return model is ICollidableModel collidableModel
                ? Create(physicsHost, collidableModel, transform) : new LocatedModel(model, transform);
        }

        public override bool SetFromCamera(PlateOffset fromCamera)
        {
            bool isChanged = base.SetFromCamera(fromCamera);
            if (isChanged)
            {
                ColliderTransform = Transform;
            }

            return isChanged;
        }

        public DynamicLocatedModel ToDynamic()
        {
            return new(Simulation, Model, Handle, BaseTransform)
            {
                Transform = Transform,
            };
        }
    }
}
