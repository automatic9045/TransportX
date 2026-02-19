using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace TransportX.Rendering
{
    public class PostProcessingPipeline : IDisposable
    {
        private readonly ID3D11DeviceContext Context;

        private readonly ID3D11VertexShader PostProcessVertexShader;
        private readonly ID3D11PixelShader PostProcessPixelShader;
        private readonly ID3D11SamplerState PointSamplerState;

        private PostProcessingBuffer? Buffer = null;

        public PostProcessingPipeline(ID3D11DeviceContext context)
        {
            Context = context;

            Blob vsBlob = ShaderFactory.CompileFromResource(Context.Device, "PostProcessVS.hlsl", "main", "VS", "vs_5_0");
            PostProcessVertexShader = Context.Device.CreateVertexShader(vsBlob);

            Blob psBlob = ShaderFactory.CompileFromResource(Context.Device, "PostProcessPS.hlsl", "main", "PS", "ps_5_0");
            PostProcessPixelShader = Context.Device.CreatePixelShader(psBlob);

            SamplerDescription pointSamplerDesc = new()
            {
                Filter = Filter.MinMagMipPoint,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
            };
            PointSamplerState = Context.Device.CreateSamplerState(pointSamplerDesc);
        }

        public void Dispose()
        {
            Buffer?.Dispose();

            PostProcessVertexShader.Dispose();
            PostProcessPixelShader.Dispose();
            PointSamplerState.Dispose();
        }

        public void Setup(ID3D11DepthStencilView depthStencil, Size size)
        {
            if (Buffer is null || size != Buffer.Size)
            {
                Buffer?.Dispose();
                Buffer = new PostProcessingBuffer(Context, depthStencil, size);
            }

            Buffer.Initialize();
        }

        public void RenderTo(ID3D11RenderTargetView renderTarget)
        {
            if (Buffer is null) throw new InvalidOperationException();

            Context.OMSetRenderTargets(renderTarget, null);

            Context.IASetInputLayout(null);
            Context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);

            Context.VSSetShader(PostProcessVertexShader);
            Context.PSSetShader(PostProcessPixelShader);

            Context.PSSetShaderResource(0, Buffer.HDRShaderResource);
            Context.PSSetSampler(0, PointSamplerState);

            Context.Draw(3, 0);

            Context.PSSetShaderResource(0, null!);
        }
    }
}
