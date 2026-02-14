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
        public required ID3D11DeviceContext DeviceContext { get; init; }
        public required ID3D11PixelShader PixelShader { get; init; }
        public required ID3D11PixelShader DebugPixelShader { get; init; }
        public required ID3D11Buffer TransformBuffer { get; init; }
        public required ID3D11Buffer MaterialBuffer { get; init; }
        public required Size ClientSize { get; init; }

        public CameraDrawContext()
        {
        }
    }
}
