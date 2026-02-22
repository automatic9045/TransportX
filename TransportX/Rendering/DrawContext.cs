using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

namespace TransportX.Rendering
{
    public readonly struct DrawContext
    {
        public required ID3D11DeviceContext DeviceContext { get; init; }
        public required ID3D11Buffer InstanceBuffer { get; init; }
        public required int InstanceCount { get; init; }
        public required ID3D11Buffer MaterialBuffer { get; init; }

        public DrawContext()
        {
        }
    }
}
