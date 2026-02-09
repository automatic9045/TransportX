using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vanara.PInvoke;
using Vortice.Direct3D11.Debug;
using Vortice.Direct3D11;
using Vortice.DXGI;

using TransportX.Rendering;

namespace TransportX.Models
{
    public class DXClient : IDXClient, IDisposable
    {
        public nint Hwnd { get; }
        public IDXGISwapChain1 SwapChain { get; }

        public ID3D11RenderTargetView? RenderTarget { get; private set; }
        public ID3D11DepthStencilView? DepthStencil { get; private set; }

        public DXClient(nint hwnd, IDXGISwapChain1 swapChain)
        {
            Hwnd = hwnd;
            SwapChain = swapChain;
        }

        public void Dispose()
        {
            RenderTarget?.Dispose();
            DepthStencil?.Dispose();
            SwapChain.Dispose();
            User32.DestroyWindow(Hwnd);
        }

        internal void Resize(ID3D11Device device, int width, int height)
        {
            RenderTarget?.Dispose();
            DepthStencil?.Dispose();

            SwapChain!.ResizeBuffers(1, (uint)width, (uint)height);

            using (ID3D11Texture2D backBuffer = SwapChain!.GetBuffer<ID3D11Texture2D>(0))
            {
                RenderTarget = device.CreateRenderTargetView(backBuffer);
            }

            Texture2DDescription depthBufferDesc = new Texture2DDescription()
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
            using (ID3D11Texture2D depthBuffer = device.CreateTexture2D(depthBufferDesc))
            {
                DepthStencilViewDescription depthStencilDesc = new DepthStencilViewDescription()
                {
                    Format = depthBufferDesc.Format,
                    ViewDimension = DepthStencilViewDimension.Texture2D,
                };
                depthStencilDesc.Texture2D.MipSlice = 0;
                DepthStencil = device.CreateDepthStencilView(depthBuffer, depthStencilDesc);
            }
        }
    }
}
