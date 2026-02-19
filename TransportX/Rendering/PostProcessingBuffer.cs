using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace TransportX.Rendering
{
    public class PostProcessingBuffer : IDisposable
    {
        private readonly ID3D11DeviceContext Context;
        private readonly ID3D11DepthStencilView DepthStencil;

        private readonly ID3D11Texture2D HDRTexture;
        private readonly ID3D11RenderTargetView HDRRenderTarget;

        public System.Drawing.Size Size { get; }
        public ID3D11ShaderResourceView HDRShaderResource { get; }

        public PostProcessingBuffer(ID3D11DeviceContext context, ID3D11DepthStencilView depthStencil, System.Drawing.Size size)
        {
            Context = context;
            DepthStencil = depthStencil;
            Size = size;

            Texture2DDescription hdrDesc = new()
            {
                Width = (uint)size.Width,
                Height = (uint)size.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R11G11B10_Float,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
            };
            HDRTexture = Context.Device.CreateTexture2D(hdrDesc);
            HDRRenderTarget = Context.Device.CreateRenderTargetView(HDRTexture);
            HDRShaderResource = Context.Device.CreateShaderResourceView(HDRTexture);
        }

        public void Dispose()
        {
            HDRTexture.Dispose();
            HDRRenderTarget.Dispose();
            HDRShaderResource.Dispose();
        }

        public void Initialize()
        {
            Context.OMSetRenderTargets(HDRRenderTarget, DepthStencil);
            Context.ClearRenderTargetView(HDRRenderTarget, Colors.Gray);
            Context.ClearDepthStencilView(DepthStencil, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
        }
    }
}
