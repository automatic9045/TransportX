using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

namespace Bus.Common.Rendering
{
    public readonly struct CameraDrawContext
    {
        public ID3D11DeviceContext DeviceContext { get; }
        public ID3D11Buffer ConstantBuffer { get; }

        public Size ClientSize { get; }
        public Vector3 Light { get; }

        public CameraDrawContext(ID3D11DeviceContext deviceContext, ID3D11Buffer constantBuffer, Size clientSize, Vector3 light)
        {
            DeviceContext = deviceContext;
            ConstantBuffer = constantBuffer;
            ClientSize = clientSize;
            Light = light;
        }
    }
}
