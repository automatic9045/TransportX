using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;

using Bus.Common.Physics;
using Bus.Common.Rendering;

namespace Bus.Common.Scenery
{
    public class DynamicLocatedModel : CollidableLocatedModel
    {
        public BodyHandle Handle { get; }
        public BodyReference Body => Simulation.Bodies[Handle];
        public bool IsKinematic => Body.Kinematic;

        protected override Matrix4x4 ColliderLocator
        {
            get => Model.Collider.OffsetInverse * Body.Pose.ToMatrix4x4() * FromCamera.TransformInverse;
            set
            {
                Simulation.Awakener.AwakenBody(Handle);
                Body.Pose = (Model.Collider.Offset * value * FromCamera.Transform).ToRigidPose();
            }
        }

        public Vector3 LinearVelocity => Vector3.TransformNormal(Body.Velocity.Linear, Model.Collider.OffsetInverse);
        public Vector3 AngularVelocity => Vector3.TransformNormal(Body.Velocity.Angular, Model.Collider.OffsetInverse);
        public BodyVelocity Velocity
        {
            get => new BodyVelocity(LinearVelocity, AngularVelocity);
            set
            {
                Vector3 linear = Vector3.TransformNormal(Body.Velocity.Linear, Model.Collider.Offset);
                Vector3 angular = Vector3.TransformNormal(Body.Velocity.Angular, Model.Collider.Offset);
                Body.Velocity = new BodyVelocity(linear, angular);
            }
        }

        internal protected DynamicLocatedModel(Simulation simulation, ICollidableModel model, BodyHandle handle, Matrix4x4 locator)
            : base(simulation, model, locator)
        {
            Handle = handle;
        }

        public static DynamicLocatedModel Create(Simulation simulation,
            ICollidableModel model, Func<ICollidableModel, RigidPose, BodyDescription> descFactory, Matrix4x4 locator)
        {
            BodyDescription desc = descFactory(model, (locator * model.Collider.Offset).ToRigidPose());
            BodyHandle handle = simulation.Bodies.Add(desc);
            return new DynamicLocatedModel(simulation, model, handle, locator);
        }

        public static DynamicLocatedModel Create(Simulation simulation,
            ICollidableModel model, float mass, CollidableDescription collidableDescription, Matrix4x4 locator)
        {
            BodyInertia inertia = model.Collider.ComputeInertia(mass);
            BodyDescription CreateDesc(ICollidableModel model, RigidPose pose)
                => BodyDescription.CreateDynamic(pose, inertia, collidableDescription, 0.01f);

            return Create(simulation, model, CreateDesc, locator);
        }

        public static DynamicLocatedModel Create(Simulation simulation, ICollidableModel model, float mass, Matrix4x4 locator)
        {
            return Create(simulation, model, mass, model.Collider.ShapeIndex, locator);
        }

        public static DynamicLocatedModel CreateKinematic(Simulation simulation, ICollidableModel model, Matrix4x4 locator)
        {
            BodyDescription CreateDesc(ICollidableModel model, RigidPose pose)
                => BodyDescription.CreateKinematic(pose, model.Collider.ShapeIndex, 0.01f);

            return Create(simulation, model, CreateDesc, locator);
        }

        public static LocatedModel CreateKinematicOrNonCollision(Simulation simulation, IModel model, Matrix4x4 locator)
        {
            return model is ICollidableModel collidableModel
                ? CreateKinematic(simulation, collidableModel, locator) : new LocatedModel(model, locator);
        }

        public void Shift(PlateOffset offset)
        {
            Locator *= offset.Transform;
        }
    }
}
