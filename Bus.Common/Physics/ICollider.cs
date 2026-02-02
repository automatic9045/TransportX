using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;
using Vortice.Direct3D11;

using Bus.Common.Rendering;

namespace Bus.Common.Physics
{
    public interface ICollider : IDebugVisualizable
    {
        IShape Shape { get; }
        TypedIndex ShapeIndex { get; }
        ColliderMaterial Material { get; }
        Pose Offset { get; }
        Pose OffsetInverse { get; }

        string? DebugName { get; set; }

        BodyInertia ComputeInertia(float mass);
    }
}
