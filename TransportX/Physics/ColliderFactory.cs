using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;

using TransportX.Physics.Colliders;

namespace TransportX.Physics
{
    public static class ColliderFactory
    {
        public static BoxCollider Box(Simulation simulation, Box shape, ColliderMaterial material, Pose offset)
        {
            return new BoxCollider(simulation, shape, material, offset);
        }

        public static CylinderCollider Cylinder(Simulation simulation, Cylinder shape, ColliderMaterial material, Pose offset)
        {
            return new CylinderCollider(simulation, shape, material, offset);
        }

        public static ColliderBase<Sphere> Sphere(Simulation simulation, Sphere shape, ColliderMaterial material, Pose offset)
        {
            return new SphereCollider(simulation, shape, material, offset);
        }

        public static ConvexHullCollider ConvexHull(Simulation simulation, ConvexHull shape, ColliderMaterial material, Pose offset)
        {
            return new ConvexHullCollider(simulation, shape, material, offset);
        }

        public static MeshCollider Mesh(Simulation simulation, Mesh shape, ColliderMaterial material, Pose offset, bool isOpen)
        {
            return new MeshCollider(simulation, shape, material, offset, isOpen);
        }
    }
}
