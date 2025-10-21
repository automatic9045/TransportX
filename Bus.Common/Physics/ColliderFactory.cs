using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;

namespace Bus.Common.Physics
{
    public static class ColliderFactory
    {
        public static Collider<Box> Box(Box shape, TypedIndex shapeIndex, Matrix4x4 offset)
            => new Collider<Box>(shape, shapeIndex, offset, (shape, mass) => shape.ComputeInertia(mass));

        public static Collider<Box> Box(Simulation simulation, Box shape, Matrix4x4 offset)
        {
            TypedIndex shapeIndex = simulation.Shapes.Add(shape);
            return Box(shape, shapeIndex, offset);
        }

        public static Collider<Cylinder> Cylinder(Cylinder shape, TypedIndex shapeIndex, Matrix4x4 offset)
            => new Collider<Cylinder>(shape, shapeIndex, offset, (shape, mass) => shape.ComputeInertia(mass));

        public static Collider<Cylinder> Cylinder(Simulation simulation, Cylinder shape, Matrix4x4 offset)
        {
            TypedIndex shapeIndex = simulation.Shapes.Add(shape);
            return Cylinder(shape, shapeIndex, offset);
        }

        public static Collider<Sphere> Sphere(Sphere shape, TypedIndex shapeIndex, Matrix4x4 offset)
            => new Collider<Sphere>(shape, shapeIndex, offset, (shape, mass) => shape.ComputeInertia(mass));

        public static Collider<Sphere> Sphere(Simulation simulation, Sphere shape, Matrix4x4 offset)
        {
            TypedIndex shapeIndex = simulation.Shapes.Add(shape);
            return Sphere(shape, shapeIndex, offset);
        }

        public static Collider<ConvexHull> ConvexHull(ConvexHull shape, TypedIndex shapeIndex, Matrix4x4 offset)
            => new Collider<ConvexHull>(shape, shapeIndex, offset, (shape, mass) => shape.ComputeInertia(mass));

        public static Collider<ConvexHull> ConvexHull(Simulation simulation, ConvexHull shape, Matrix4x4 offset)
        {
            TypedIndex shapeIndex = simulation.Shapes.Add(shape);
            return ConvexHull(shape, shapeIndex, offset);
        }

        public static Collider<Mesh> Mesh(Mesh shape, TypedIndex shapeIndex, Matrix4x4 offset, bool isOpen)
            => new Collider<Mesh>(shape, shapeIndex, offset,
                isOpen ? (shape, mass) => shape.ComputeOpenInertia(mass) : (shape, mass) => shape.ComputeClosedInertia(mass));

        public static Collider<Mesh> Mesh(Simulation simulation, Mesh shape, Matrix4x4 offset, bool isOpen)
        {
            TypedIndex shapeIndex = simulation.Shapes.Add(shape);
            return Mesh(shape, shapeIndex, offset, isOpen);
        }
    }
}
