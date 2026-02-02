using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;

using Bus.Common.Physics.Colliders;

namespace Bus.Common.Physics
{
    public static class ColliderFactory
    {
        public static BoxCollider Box(Box shape, TypedIndex shapeIndex, ColliderMaterial material, Pose offset)
            => new BoxCollider(shape, shapeIndex, material, offset);

        public static BoxCollider Box(Simulation simulation, Box shape, ColliderMaterial material, Pose offset)
        {
            TypedIndex shapeIndex = simulation.Shapes.Add(shape);
            return Box(shape, shapeIndex, material, offset);
        }

        public static CylinderCollider Cylinder(Cylinder shape, TypedIndex shapeIndex, ColliderMaterial material, Pose offset)
            => new CylinderCollider(shape, shapeIndex, material, offset);

        public static CylinderCollider Cylinder(Simulation simulation, Cylinder shape, ColliderMaterial material, Pose offset)
        {
            TypedIndex shapeIndex = simulation.Shapes.Add(shape);
            return Cylinder(shape, shapeIndex, material, offset);
        }

        public static Collider<Sphere> Sphere(Sphere shape, TypedIndex shapeIndex, ColliderMaterial material, Pose offset)
            => new Collider<Sphere>(shape, shapeIndex, material, offset, (shape, mass) => shape.ComputeInertia(mass));

        public static Collider<Sphere> Sphere(Simulation simulation, Sphere shape, ColliderMaterial material, Pose offset)
        {
            TypedIndex shapeIndex = simulation.Shapes.Add(shape);
            return Sphere(shape, shapeIndex, material, offset);
        }

        public static ConvexHullCollider ConvexHull(ConvexHull shape, TypedIndex shapeIndex, ColliderMaterial material, Pose offset)
            => new ConvexHullCollider(shape, shapeIndex, material, offset);

        public static ConvexHullCollider ConvexHull(Simulation simulation, ConvexHull shape, ColliderMaterial material, Pose offset)
        {
            TypedIndex shapeIndex = simulation.Shapes.Add(shape);
            return ConvexHull(shape, shapeIndex, material, offset);
        }

        public static MeshCollider Mesh(Mesh shape, TypedIndex shapeIndex, ColliderMaterial material, Pose offset, bool isOpen)
            => new MeshCollider(shape, shapeIndex, material, offset, isOpen);

        public static MeshCollider Mesh(Simulation simulation, Mesh shape, ColliderMaterial material, Pose offset, bool isOpen)
        {
            TypedIndex shapeIndex = simulation.Shapes.Add(shape);
            return Mesh(shape, shapeIndex, material, offset, isOpen);
        }
    }
}
