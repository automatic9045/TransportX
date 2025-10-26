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
    public class Collider<TShape> : ICollider where TShape : unmanaged, IShape
    {
        protected readonly Func<TShape, float, BodyInertia> InertiaFactory;

        public TShape Shape { get; }
        IShape ICollider.Shape => Shape;
        public TypedIndex ShapeIndex { get; }
        public Material Material { get; }
        public Matrix4x4 Offset { get; }
        public Matrix4x4 OffsetInverse { get; }

        public Collider(TShape shape, TypedIndex shapeIndex, Material material, Matrix4x4 offset, Func<TShape, float, BodyInertia> inertiaFactory)
        {
            Shape = shape;
            ShapeIndex = shapeIndex;
            Material = material;
            Offset = offset;
            Matrix4x4.Invert(offset, out Matrix4x4 offsetInverse);
            OffsetInverse = offsetInverse;
            InertiaFactory = inertiaFactory;
        }

        public BodyInertia ComputeInertia(float mass)
        {
            return InertiaFactory(Shape, mass);
        }
    }
}
