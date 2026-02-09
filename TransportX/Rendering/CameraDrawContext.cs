using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

namespace TransportX.Rendering
{
    public readonly struct CameraDrawContext
    {
        public ID3D11DeviceContext DeviceContext { get; }
        public ID3D11Buffer VertexConstantBuffer { get; }
        public ID3D11Buffer PixelConstantBuffer { get; }
        public Size ClientSize { get; }
        public Vector3 Light { get; }

        public CameraDrawContext(ID3D11DeviceContext deviceContext,
            ID3D11Buffer vertexConstantBuffer, ID3D11Buffer pixelConstantBuffer, Size clientSize, Vector3 light)
        {
            DeviceContext = deviceContext;
            VertexConstantBuffer = vertexConstantBuffer;
            PixelConstantBuffer = pixelConstantBuffer;
            ClientSize = clientSize;
            Light = light;
        }
    }
}
