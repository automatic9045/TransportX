using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;
using Vortice.Direct3D11;

using TransportX.Rendering;

namespace TransportX.Physics
{
    public interface ICollider
    {
        IShape Shape { get; }
        TypedIndex ShapeIndex { get; }
        ColliderMaterial Material { get; }
        Pose Offset { get; }
        Pose OffsetInverse { get; }

        BodyInertia ComputeInertia(float mass);
        IDebugModel CreateDebugModel(ID3D11Device device);
    }
}
