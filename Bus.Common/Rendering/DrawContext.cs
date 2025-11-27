using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

namespace Bus.Common.Rendering
{
    public readonly struct DrawContext
    {
        public ID3D11DeviceContext DeviceContext { get; }
        public ID3D11Buffer VertexConstantBuffer { get; }
        public ID3D11Buffer PixelConstantBuffer { get; }

        public DrawContext(ID3D11DeviceContext deviceContext, ID3D11Buffer vertexConstantBuffer, ID3D11Buffer pixelConstantBuffer)
        {
            DeviceContext = deviceContext;
            VertexConstantBuffer = vertexConstantBuffer;
            PixelConstantBuffer = pixelConstantBuffer;
        }
    }
}
