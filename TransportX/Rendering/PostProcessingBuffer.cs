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
        public const int BloomMipCount = 5;

        private readonly ID3D11DeviceContext Context;
        private readonly ID3D11DepthStencilView DepthStencil;

        public System.Drawing.Size Size { get; }
        public RenderTexture HdrBuffer { get; }
        public IReadOnlyList<RenderTexture> BloomMips { get; }

        public PostProcessingBuffer(ID3D11DeviceContext context, ID3D11DepthStencilView depthStencil, System.Drawing.Size size)
        {
            Context = context;
            DepthStencil = depthStencil;
            Size = size;

            Texture2DDescription desc = new()
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

            HdrBuffer = new RenderTexture(Context.Device, desc);

            RenderTexture[] bloomMips = new RenderTexture[BloomMipCount];
            uint mipWidth = (uint)size.Width / 2;
            uint mipHeight = (uint)size.Height / 2;

            for (int i = 0; i < BloomMipCount; i++)
            {
                desc.Width = uint.Max(1, mipWidth);
                desc.Height = uint.Max(1, mipHeight);

                bloomMips[i] = new RenderTexture(Context.Device, desc);

                mipWidth /= 2;
                mipHeight /= 2;
            }

            BloomMips = bloomMips;
        }

        public void Dispose()
        {
            HdrBuffer.Dispose();

            for (int i = 0; i < BloomMipCount; i++)
            {
                BloomMips[i].Dispose();
            }
        }

        public void Initialize()
        {
            Context.OMSetRenderTargets(HdrBuffer.RenderTargetView, DepthStencil);
            Context.ClearRenderTargetView(HdrBuffer.RenderTargetView, Colors.Gray);
            Context.ClearDepthStencilView(DepthStencil, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
        }
    }
}
