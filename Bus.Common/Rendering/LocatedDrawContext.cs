using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

using Bus.Common.Scenery;

namespace Bus.Common.Rendering
{
    public readonly struct LocatedDrawContext
    {
        public required ID3D11DeviceContext DeviceContext { get; init; }
        public required ID3D11Buffer VertexConstantBuffer { get; init; }
        public required ID3D11Buffer PixelConstantBuffer { get; init; }
        public required PlateOffset PlateOffset { get; init; }
        public required Matrix4x4 View { get; init; }
        public required Matrix4x4 Projection { get; init; }
        public Vector3 Light { get; init; } = Vector3.Zero;
        public bool DrawColliderDebugModel { get; init; } = true;

        public LocatedDrawContext()
        {
        }
    }
}
