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
    public interface ICollider
    {
        IShape Shape { get; }
        TypedIndex ShapeIndex { get; }
        Material Material { get; }
        Matrix4x4 Offset { get; }
        Matrix4x4 OffsetInverse { get; }

        BodyInertia ComputeInertia(float mass);
    }
}
