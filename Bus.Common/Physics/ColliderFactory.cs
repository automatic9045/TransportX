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
        public static Collider<Box> Box(Box shape, TypedIndex shapeIndex, Matrix4x4 transform)
            => new Collider<Box>(shape, shapeIndex, transform, (shape, mass) => shape.ComputeInertia(mass));

        public static Collider<Box> Box(Simulation simulation, Box shape, Matrix4x4 transform)
        {
            TypedIndex shapeIndex = simulation.Shapes.Add(shape);
            return Box(shape, shapeIndex, transform);
        }

        public static Collider<Sphere> Sphere(Sphere shape, TypedIndex shapeIndex, Matrix4x4 transform)
            => new Collider<Sphere>(shape, shapeIndex, transform, (shape, mass) => shape.ComputeInertia(mass));

        public static Collider<Sphere> Sphere(Simulation simulation, Sphere shape, Matrix4x4 transform)
        {
            TypedIndex shapeIndex = simulation.Shapes.Add(shape);
            return Sphere(shape, shapeIndex, transform);
        }

        public static Collider<Mesh> Mesh(Mesh shape, TypedIndex shapeIndex, Matrix4x4 transform, bool isOpen)
            => new Collider<Mesh>(shape, shapeIndex, transform,
                isOpen ? (shape, mass) => shape.ComputeOpenInertia(mass) : (shape, mass) => shape.ComputeClosedInertia(mass));

        public static Collider<Mesh> Mesh(Simulation simulation, Mesh shape, Matrix4x4 transform, bool isOpen)
        {
            TypedIndex shapeIndex = simulation.Shapes.Add(shape);
            return Mesh(shape, shapeIndex, transform, isOpen);
        }
    }
}
