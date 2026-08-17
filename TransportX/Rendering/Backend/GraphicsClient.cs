using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;
using Vortice.DXGI;

namespace TransportX.Rendering.Backend
{
    public class GraphicsClient : IGraphicsClient, IDisposable
    {
        public nint Hwnd { get; }
        public IDXGISwapChain1 SwapChain { get; }
        public RenderSurface? Surface { get; private set; } = null;

        public GraphicsClient(nint hwnd, IDXGISwapChain1 swapChain)
        {
            Hwnd = hwnd;
            SwapChain = swapChain;
        }

        public void Dispose()
        {
            if (Surface is not null)
            {
                Surface.Value.RenderTarget.Dispose();
                Surface.Value.DepthStencil.Dispose();
            }
            SwapChain.Dispose();
        }

        public void Resize(ID3D11Device device, int width, int height)
        {
            if (Surface is not null)
            {
                Surface.Value.RenderTarget.Dispose();
                Surface.Value.DepthStencil.Dispose();
            }
            Surface = null;

            SwapChain!.ResizeBuffers(0, (uint)width, (uint)height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);

            using ID3D11Texture2D backBuffer = SwapChain!.GetBuffer<ID3D11Texture2D>(0);
            RenderTargetViewDescription renderTargetDesc = new()
            {
                Format = Format.R8G8B8A8_UNorm_SRgb,
                ViewDimension = RenderTargetViewDimension.Texture2D,
            };
            ID3D11RenderTargetView renderTarget = device.CreateRenderTargetView(backBuffer, renderTargetDesc);

            Texture2DDescription depthBufferDesc = new()
            {
                Format = Format.D32_Float_S8X24_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = (uint)width,
                Height = (uint)height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.None,
            };
            using ID3D11Texture2D depthBuffer = device.CreateTexture2D(depthBufferDesc);

            DepthStencilViewDescription depthStencilDesc = new()
            {
                Format = depthBufferDesc.Format,
                ViewDimension = DepthStencilViewDimension.Texture2D,
            };
            depthStencilDesc.Texture2D.MipSlice = 0;
            ID3D11DepthStencilView depthStencil = device.CreateDepthStencilView(depthBuffer, depthStencilDesc);

            Surface = new RenderSurface(renderTarget, depthStencil);
        }
    }
}
