using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;
using Vortice.DXGI;

namespace TransportX.Rendering.Backend
{
    public interface IGraphicsClient : IDisposable
    {
        nint Hwnd { get; }
        IDXGISwapChain1 SwapChain { get; }
        RenderSurface? Surface { get; }
    }
}
