using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;

using Bus.Common.Rendering;
using Vortice.Direct3D11;

namespace Bus.Common.Physics
{
    public interface ICollider : IDisposable
    {
        IShape Shape { get; }
        TypedIndex ShapeIndex { get; }
        ColliderMaterial Material { get; }
        Matrix4x4 Offset { get; }
        Matrix4x4 OffsetInverse { get; }

        IModel? DebugModel { get; }

        void IDisposable.Dispose()
        {
            DebugModel?.Dispose();
        }

        BodyInertia ComputeInertia(float mass);
        void CreateDebugModel(ID3D11Device device, Vector4 color);
    }
}
