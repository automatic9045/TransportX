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
        public override Matrix4x4 Transform
        {
            get => ColliderTransform;
            set => ColliderTransform = value;
        }

        internal protected DynamicLocatedModel(Simulation simulation, ICollidableModel model, BodyHandle handle, Matrix4x4 transform)
            : base(simulation, model, handle, transform)
        {
        }

        public static DynamicLocatedModel Create(IPhysicsHost physicsHost,
            ICollidableModel model, Func<ICollidableModel, RigidPose, BodyDescription> descFactory, Matrix4x4 transform)
        {
            BodyDescription desc = descFactory(model, (model.Collider.Offset * transform).ToRigidPose());
            BodyHandle handle = physicsHost.Simulation.Bodies.Add(desc);
            physicsHost.SetMaterial(handle, model.Collider.Material);
            return new DynamicLocatedModel(physicsHost.Simulation, model, handle, transform);
        }

        public static DynamicLocatedModel Create(IPhysicsHost physicsHost,
            ICollidableModel model, float mass, CollidableDescription collidableDescription, Matrix4x4 transform)
        {
            BodyInertia inertia = model.Collider.ComputeInertia(mass);
            BodyDescription CreateDesc(ICollidableModel model, RigidPose pose)
                => BodyDescription.CreateDynamic(pose, inertia, collidableDescription, 0.01f);

            return Create(physicsHost, model, CreateDesc, transform);
        }

        public static DynamicLocatedModel Create(IPhysicsHost physicsHost, ICollidableModel model, float mass, Matrix4x4 transform)
        {
            return Create(physicsHost, model, mass, model.Collider.ShapeIndex, transform);
        }

        public override bool SetFromCamera(PlateOffset fromCamera)
        {
            PlateOffset delta = fromCamera - FromCamera;

            bool isChanged = base.SetFromCamera(fromCamera);
            if (isChanged)
            {
                Simulation.Awakener.AwakenBody(Handle);
                Body.Pose.Position += delta.Position;
            }

            return isChanged;
        }

        public void Shift(PlateOffset offset)
        {
            Transform *= offset.PoseInverse.ToMatrix4x4();
        }
    }
}
