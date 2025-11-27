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
        public ID3D11DeviceContext DeviceContext { get; }
        public ID3D11Buffer VertexConstantBuffer { get; }
        public ID3D11Buffer PixelConstantBuffer { get; }
        public PlateOffset PlateOffset { get; }
        public Matrix4x4 View { get; }
        public Matrix4x4 Projection { get; }
        public Vector3 Light { get; }

        public LocatedDrawContext(ID3D11DeviceContext deviceContext, ID3D11Buffer vertexConstantBuffer, ID3D11Buffer pixelConstantBuffer,
            PlateOffset plateOffset, Matrix4x4 view, Matrix4x4 projection, Vector3 light)
        {
            DeviceContext = deviceContext;
            VertexConstantBuffer = vertexConstantBuffer;
            PixelConstantBuffer = pixelConstantBuffer;
            PlateOffset = plateOffset;
            View = view;
            Projection = projection;
            Light = light;
        }
    }
}
