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
        public static Collider<Box> Box(Box shape, TypedIndex shapeIndex, Material material, Matrix4x4 offset)
            => new Collider<Box>(shape, shapeIndex, material, offset, (shape, mass) => shape.ComputeInertia(mass));

        public static Collider<Box> Box(Simulation simulation, Box shape, Material material, Matrix4x4 offset)
        {
            TypedIndex shapeIndex = simulation.Shapes.Add(shape);
            return Box(shape, shapeIndex, material, offset);
        }

        public static Collider<Cylinder> Cylinder(Cylinder shape, TypedIndex shapeIndex, Material material, Matrix4x4 offset)
            => new Collider<Cylinder>(shape, shapeIndex, material, offset, (shape, mass) => shape.ComputeInertia(mass));

        public static Collider<Cylinder> Cylinder(Simulation simulation, Cylinder shape, Material material, Matrix4x4 offset)
        {
            TypedIndex shapeIndex = simulation.Shapes.Add(shape);
            return Cylinder(shape, shapeIndex, material, offset);
        }

        public static Collider<Sphere> Sphere(Sphere shape, TypedIndex shapeIndex, Material material, Matrix4x4 offset)
            => new Collider<Sphere>(shape, shapeIndex, material, offset, (shape, mass) => shape.ComputeInertia(mass));

        public static Collider<Sphere> Sphere(Simulation simulation, Sphere shape, Material material, Matrix4x4 offset)
        {
            TypedIndex shapeIndex = simulation.Shapes.Add(shape);
            return Sphere(shape, shapeIndex, material, offset);
        }

        public static ConvexHullCollider ConvexHull(ConvexHull shape, TypedIndex shapeIndex, Material material, Matrix4x4 offset)
            => new ConvexHullCollider(shape, shapeIndex, material, offset);

        public static ConvexHullCollider ConvexHull(Simulation simulation, ConvexHull shape, Material material, Matrix4x4 offset)
        {
            TypedIndex shapeIndex = simulation.Shapes.Add(shape);
            return ConvexHull(shape, shapeIndex, material, offset);
        }

        public static MeshCollider Mesh(Mesh shape, TypedIndex shapeIndex, Material material, Matrix4x4 offset, bool isOpen)
            => new MeshCollider(shape, shapeIndex, material, offset, isOpen);

        public static MeshCollider Mesh(Simulation simulation, Mesh shape, Material material, Matrix4x4 offset, bool isOpen)
        {
            TypedIndex shapeIndex = simulation.Shapes.Add(shape);
            return Mesh(shape, shapeIndex, material, offset, isOpen);
        }
    }
}
