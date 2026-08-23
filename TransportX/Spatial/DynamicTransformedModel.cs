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
    public class DynamicTransformedModel : BodyTransformedModel
    {
        protected DynamicTransformedModel(IPhysicsHost physicsHost, in ModelResourceSet resource, BodyDescription description, Pose basePose)
            : base(physicsHost, resource, description, basePose)
        {
        }

        public static DynamicTransformedModel Create(IPhysicsHost physicsHost,
            in ModelResourceSet resource, Func<ModelResourceSet, RigidPose, BodyDescription> descFactory, Pose basePose)
        {
            BodyDescription desc = descFactory(resource, (resource.GetCollider().Offset * basePose).ToRigidPose());
            return new DynamicTransformedModel(physicsHost, resource, desc, basePose);
        }

        public static DynamicTransformedModel Create(IPhysicsHost physicsHost,
            in ModelResourceSet resource, float mass, CollidableDescription collidableDescription, Pose basePose)
        {
            BodyInertia inertia = resource.GetCollider().ComputeInertia(mass);
            return Create(physicsHost, resource, CreateDesc, basePose);


            BodyDescription CreateDesc(ModelResourceSet model, RigidPose pose)
                => BodyDescription.CreateDynamic(pose, inertia, collidableDescription, 0.01f);
        }

        public static DynamicTransformedModel Create(IPhysicsHost physicsHost, in ModelResourceSet resource, float mass, Pose basePose)
        {
            return Create(physicsHost, resource, mass, resource.GetCollider().ShapeIndex, basePose);
        }
    }
}
