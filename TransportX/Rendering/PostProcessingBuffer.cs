using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

        public Vector2 Size { get; }

        public RenderTexture AmbientBuffer { get; }
        public RenderTexture DirectionalBuffer { get; }
        public RenderTexture RawShadowBuffer { get; }

        public RenderTexture ResolvedHdrBuffer { get; }

        public IReadOnlyList<RenderTexture> BloomMips { get; }
        public RenderTexture LdrBuffer { get; }

        public PostProcessingBuffer(ID3D11DeviceContext context, ID3D11DepthStencilView depthStencil, Vector2 size)
        {
            Context = context;
            DepthStencil = depthStencil;
            Size = size;

            Texture2DDescription desc = new()
            {
                Width = (uint)size.X,
                Height = (uint)size.Y,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
            };

            desc.Format = Format.R11G11B10_Float;
            AmbientBuffer = new RenderTexture(Context.Device, desc);
            DirectionalBuffer = new RenderTexture(Context.Device, desc);
            ResolvedHdrBuffer = new RenderTexture(Context.Device, desc);

            desc.Format = Format.R16_Float;
            RawShadowBuffer = new RenderTexture(Context.Device, desc);

            desc.Format = Format.R11G11B10_Float;
            RenderTexture[] bloomMips = new RenderTexture[BloomMipCount];
            uint mipWidth = (uint)size.X / 2;
            uint mipHeight = (uint)size.Y / 2;
            for (int i = 0; i < BloomMipCount; i++)
            {
                desc.Width = uint.Max(1, mipWidth);
                desc.Height = uint.Max(1, mipHeight);

                bloomMips[i] = new RenderTexture(Context.Device, desc);

                mipWidth /= 2;
                mipHeight /= 2;
            }
            BloomMips = bloomMips;

            desc = desc with
            {
                Width = (uint)size.X,
                Height = (uint)size.Y,
                Format = Format.R8G8B8A8_UNorm,
            };
            LdrBuffer = new RenderTexture(Context.Device, desc);
        }

        public void Dispose()
        {
            AmbientBuffer.Dispose();
            DirectionalBuffer.Dispose();
            RawShadowBuffer.Dispose();
            ResolvedHdrBuffer.Dispose();

            for (int i = 0; i < BloomMipCount; i++)
            {
                BloomMips[i].Dispose();
            }

            LdrBuffer.Dispose();
        }

        public void Initialize()
        {
            ReadOnlySpan<ID3D11RenderTargetView> renderTargets = [
                AmbientBuffer.RenderTargetView,
                DirectionalBuffer.RenderTargetView,
                RawShadowBuffer.RenderTargetView
            ];
            Context.OMSetRenderTargets(renderTargets, DepthStencil);

            Context.ClearRenderTargetView(AmbientBuffer.RenderTargetView, Colors.Gray);
            Context.ClearRenderTargetView(DirectionalBuffer.RenderTargetView, Colors.Black);
            Context.ClearRenderTargetView(RawShadowBuffer.RenderTargetView, Colors.White);
            Context.ClearDepthStencilView(DepthStencil, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
        }
    }
}
