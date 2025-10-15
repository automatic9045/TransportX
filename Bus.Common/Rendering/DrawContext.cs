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
    public readonly struct DrawContext
    {
        public ID3D11DeviceContext DeviceContext { get; }
        public ID3D11Buffer ConstantBuffer { get; }
        public PlateOffset PlateOffset { get; }
        public Matrix4x4 View { get; }
        public Matrix4x4 Projection { get; }

        public DrawContext(ID3D11DeviceContext deviceContext, ID3D11Buffer constantBuffer, PlateOffset plateOffset, Matrix4x4 view, Matrix4x4 projection)
        {
            DeviceContext = deviceContext;
            ConstantBuffer = constantBuffer;
            PlateOffset = plateOffset;
            View = view;
            Projection = projection;
        }
    }
}
