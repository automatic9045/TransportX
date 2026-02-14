using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

namespace TransportX.Rendering
{
    public readonly struct DrawContext
    {
        public ID3D11DeviceContext DeviceContext { get; }
        public ID3D11Buffer TransformBuffer { get; }
        public ID3D11Buffer MaterialBuffer { get; }

        public DrawContext(ID3D11DeviceContext deviceContext, ID3D11Buffer transformBuffer, ID3D11Buffer materialBuffer)
        {
            DeviceContext = deviceContext;
            TransformBuffer = transformBuffer;
            MaterialBuffer = materialBuffer;
        }
    }
}
