using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D;
using Vortice.Direct3D11;

using TransportX.Environment;

namespace TransportX.Rendering
{
    public class PostProcessingPipeline : IDisposable
    {
        private readonly ID3D11DeviceContext Context;

        private readonly ID3D11VertexShader VertexShader;

        private readonly ID3D11PixelShader ExtractPixelShader;
        private readonly ID3D11PixelShader DownsamplePixelShader;
        private readonly ID3D11PixelShader UpsamplePixelShader;
        private readonly ID3D11PixelShader CompositePixelShader;

        private readonly ID3D11SamplerState SamplerState;

        private readonly ID3D11Buffer PostProcessBuffer;
        private readonly ID3D11Buffer BlurBuffer;

        private readonly ID3D11BlendState AdditiveBlendState;
        private readonly ID3D11BlendState OpaqueBlendState;

        private PostProcessingBuffer? Buffer = null;

        public PostProcessingPipeline(ID3D11DeviceContext context)
        {
            Context = context;

            Blob vsBlob = ShaderFactory.CompileFromResource(Context.Device, "PostProcess.VS.hlsl", "main", "VS", "vs_5_0");
            VertexShader = Context.Device.CreateVertexShader(vsBlob);

            Blob extractPSBlob = ShaderFactory.CompileFromResource(Context.Device, "PostProcess.ExtractPS.hlsl", "main", "PS", "ps_5_0");
            ExtractPixelShader = Context.Device.CreatePixelShader(extractPSBlob);

            Blob downsamplePSBlob = ShaderFactory.CompileFromResource(Context.Device, "PostProcess.DownsamplePS.hlsl", "main", "PS", "ps_5_0");
            DownsamplePixelShader = Context.Device.CreatePixelShader(downsamplePSBlob);

            Blob upsamplePSBlob = ShaderFactory.CompileFromResource(Context.Device, "PostProcess.UpsamplePS.hlsl", "main", "PS", "ps_5_0");
            UpsamplePixelShader = Context.Device.CreatePixelShader(upsamplePSBlob);

            Blob compositePSBlob = ShaderFactory.CompileFromResource(Context.Device, "PostProcess.CompositePS.hlsl", "main", "PS", "ps_5_0");
            CompositePixelShader = Context.Device.CreatePixelShader(compositePSBlob);

            SamplerDescription samplerDesc = new()
            {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
            };
            SamplerState = Context.Device.CreateSamplerState(samplerDesc);

            BufferDescription postProcessBufferDesc = new()
            {
                Usage = ResourceUsage.Default,
                ByteWidth = (uint)PostProcessConstants.Size,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = 0,
            };
            PostProcessBuffer = Context.Device.CreateBuffer(postProcessBufferDesc);

            BufferDescription blurBufferDesc = new()
            {
                Usage = ResourceUsage.Default,
                ByteWidth = (uint)BlurConstants.Size,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = 0,
            };
            BlurBuffer = Context.Device.CreateBuffer(blurBufferDesc);

            BlendDescription additiveBlendDesc = new()
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false,
            };
            additiveBlendDesc.RenderTarget[0] = new RenderTargetBlendDescription()
            {
                BlendEnable = true,
                SourceBlend = Blend.One,
                DestinationBlend = Blend.One,
                BlendOperation = BlendOperation.Add,
                SourceBlendAlpha = Blend.One,
                DestinationBlendAlpha = Blend.One,
                BlendOperationAlpha = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteEnable.All,
            };
            AdditiveBlendState = Context.Device.CreateBlendState(additiveBlendDesc);

            BlendDescription opaqueBlendDesc = new();
            opaqueBlendDesc.RenderTarget[0] = new RenderTargetBlendDescription()
            {
                BlendEnable = false,
                RenderTargetWriteMask = ColorWriteEnable.All,
            };
            OpaqueBlendState = Context.Device.CreateBlendState(opaqueBlendDesc);
        }

        public void Dispose()
        {
            Buffer?.Dispose();

            VertexShader.Dispose();

            ExtractPixelShader.Dispose();
            DownsamplePixelShader.Dispose();
            UpsamplePixelShader.Dispose();
            CompositePixelShader.Dispose();

            SamplerState.Dispose();

            PostProcessBuffer.Dispose();
            BlurBuffer.Dispose();

            AdditiveBlendState.Dispose();
            OpaqueBlendState.Dispose();
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

        public void RenderTo(ID3D11RenderTargetView renderTarget, EnvironmentProfile environment)
        {
            if (Buffer is null) throw new InvalidOperationException();

            PostProcessConstants postProcessConstants = new()
            {
                BloomThreshold = environment.Bloom.Threshold,
                BloomIntensity = environment.Bloom.Intensity,
                BloomScatter = environment.Bloom.Scatter,
                BloomSoftKnee = environment.Bloom.SoftKnee,
                BloomTint = environment.Bloom.Tint.ToLinear(),
            };
            Context.UpdateSubresource(postProcessConstants, PostProcessBuffer);

            Context.IASetInputLayout(null);
            Context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
            Context.VSSetShader(VertexShader);
            Context.PSSetSampler(0, SamplerState);
            Context.PSSetConstantBuffer(0, PostProcessBuffer);
            Context.PSSetConstantBuffer(1, BlurBuffer);


            // 1. Extract

            Context.OMSetBlendState(OpaqueBlendState);
            Context.OMSetRenderTargets(Buffer.BloomMips[0].RenderTargetView, null);
            Context.RSSetViewport(0, 0, Buffer.BloomMips[0].Size.Width, Buffer.BloomMips[0].Size.Height);

            Context.PSSetShader(ExtractPixelShader);
            Context.PSSetShaderResource(0, Buffer.HdrBuffer.ShaderResourceView);
            Context.Draw(3, 0);
            Context.PSSetShaderResource(0, null!);


            // 2. Downsample

            Context.PSSetShader(DownsamplePixelShader);

            for (int i = 0; i < PostProcessingBuffer.BloomMipCount - 1; i++)
            {
                int next = i + 1;
                Context.OMSetRenderTargets(Buffer.BloomMips[next].RenderTargetView, null);
                Context.RSSetViewport(0, 0, Buffer.BloomMips[next].Size.Width, Buffer.BloomMips[next].Size.Height);

                BlurConstants blur = new()
                {
                    TexelSize = new Vector2(1f / Buffer.BloomMips[i].Size.Width, 1f / Buffer.BloomMips[i].Size.Height),
                    BloomScatter = 1.0f,
                };
                Context.UpdateSubresource(blur, BlurBuffer);

                Context.PSSetShaderResource(0, Buffer.BloomMips[i].ShaderResourceView);
                Context.Draw(3, 0);
                Context.PSSetShaderResource(0, null!);
            }


            // 3. Upsample

            Context.OMSetBlendState(AdditiveBlendState);
            Context.PSSetShader(UpsamplePixelShader);

            for (int i = PostProcessingBuffer.BloomMipCount - 1; 0 < i; i--)
            {
                int prev = i - 1;
                Context.OMSetRenderTargets(Buffer.BloomMips[prev].RenderTargetView, null);
                Context.RSSetViewport(0, 0, Buffer.BloomMips[prev].Size.Width, Buffer.BloomMips[prev].Size.Height);

                BlurConstants blur = new()
                {
                    TexelSize = new Vector2(1f / Buffer.BloomMips[i].Size.Width, 1f / Buffer.BloomMips[i].Size.Height),
                    BloomScatter = postProcessConstants.BloomScatter,
                };
                Context.UpdateSubresource(blur, BlurBuffer);

                Context.PSSetShaderResource(0, Buffer.BloomMips[i].ShaderResourceView);
                Context.Draw(3, 0);
                Context.PSSetShaderResource(0, null!);
            }


            // 4. Composite

            Context.OMSetBlendState(OpaqueBlendState);
            Context.OMSetRenderTargets(renderTarget, null);
            Context.RSSetViewport(0, 0, Buffer.Size.Width, Buffer.Size.Height);

            Context.PSSetShader(CompositePixelShader);
            Context.PSSetShaderResource(0, Buffer.HdrBuffer.ShaderResourceView);
            Context.PSSetShaderResource(1, Buffer.BloomMips[0].ShaderResourceView);
            Context.Draw(3, 0);


            Context.PSSetShaderResource(0, null!);
            Context.PSSetShaderResource(1, null!);
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct BlurConstants
        {
            internal static readonly int Size = Marshal.SizeOf<BlurConstants>();


            public Vector2 TexelSize;
            public float BloomScatter;
            public float Padding;

            public BlurConstants()
            {
            }
        }
    }
}
