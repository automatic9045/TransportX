using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11.Debug;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace TransportX.Rendering
{
    public interface IDXClient
    {
        nint Hwnd { get; }
        IDXGISwapChain1 SwapChain { get; }

        ID3D11RenderTargetView? RenderTarget { get; }
        ID3D11DepthStencilView? DepthStencil { get; }
    }
}
