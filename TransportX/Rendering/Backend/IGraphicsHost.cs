using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;
using Vortice.DXGI;

namespace TransportX.Rendering.Backend
{
    public interface IGraphicsHost : IDisposable
    {
        ID3D11Device Device { get; }
        ID3D11DeviceContext Context { get; }
        IDXGIFactory2 DXGIFactory { get; }

        event EventHandler? Disposing;
    }
}
