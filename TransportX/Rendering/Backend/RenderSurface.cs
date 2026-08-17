using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

namespace TransportX.Rendering.Backend
{
    public readonly struct RenderSurface
    {
        public required ID3D11RenderTargetView RenderTarget { get; init; }
        public required ID3D11DepthStencilView DepthStencil { get; init; }

        [SetsRequiredMembers]
        public RenderSurface(ID3D11RenderTargetView renderTarget, ID3D11DepthStencilView depthStencil)
        {
            RenderTarget = renderTarget;
            DepthStencil = depthStencil;
        }
    }
}
