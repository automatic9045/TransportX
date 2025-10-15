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
        public Matrix4x4 Transform { get; }
        public Matrix4x4 TransformInverse { get; }

        public Collider(TShape shape, TypedIndex shapeIndex, Matrix4x4 transform, Func<TShape, float, BodyInertia> inertiaFactory)
        {
            Shape = shape;
            ShapeIndex = shapeIndex;
            Transform = transform;
            Matrix4x4.Invert(transform, out Matrix4x4 transformInverse);
            TransformInverse = transformInverse;
            InertiaFactory = inertiaFactory;
        }

        public BodyInertia ComputeInertia(float mass)
        {
            return InertiaFactory(Shape, mass);
        }
    }
}
