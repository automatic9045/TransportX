using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.Mathematics;

using TransportX.Environment;
using TransportX.Rendering.Backend;

namespace TransportX.Rendering.Pipelines
{
    public class PostProcessingPipeline : IDisposable
    {
        private const float AdaptationSpeed = 2;


        private readonly ID3D11DeviceContext Context;

        private readonly ID3D11VertexShader VertexShader;

        private readonly ID3D11PixelShader DeferredLightingPixelShader;
        private readonly ID3D11PixelShader LuminanceExtractPixelShader;
        private readonly ID3D11PixelShader ExtractPixelShader;
        private readonly ID3D11PixelShader DownsamplePixelShader;
        private readonly ID3D11PixelShader UpsamplePixelShader;
        private readonly ID3D11PixelShader CompositePixelShader;
        private readonly ID3D11PixelShader FxaaPixelShader;

        private readonly ID3D11SamplerState SamplerState;

        private readonly ID3D11Buffer PostProcessBuffer;
        private readonly ID3D11Buffer BlurBuffer;

        private readonly ID3D11BlendState AdditiveBlendState;
        private readonly ID3D11BlendState OpaqueBlendState;

        private PostProcessingBuffer? Buffer = null;

        private float Exposure = 0;

        public PostProcessingPipeline(ID3D11DeviceContext context)
        {
            Context = context;

            Blob vsBlob = ShaderFactory.CompileFromResource("PostProcess.VS.hlsl", "main", "VS", "vs_5_0");
            VertexShader = Context.Device.CreateVertexShader(vsBlob);

            Blob deferredLightingPSBlob = ShaderFactory.CompileFromResource("PostProcess.DeferredLightingPS.hlsl", "main", "PS", "ps_5_0");
            DeferredLightingPixelShader = Context.Device.CreatePixelShader(deferredLightingPSBlob);

            Blob luminanceExtractPSBlob = ShaderFactory.CompileFromResource("PostProcess.LuminanceExtractPS.hlsl", "main", "PS", "ps_5_0");
            LuminanceExtractPixelShader = Context.Device.CreatePixelShader(luminanceExtractPSBlob);

            Blob extractPSBlob = ShaderFactory.CompileFromResource("PostProcess.ExtractPS.hlsl", "main", "PS", "ps_5_0");
            ExtractPixelShader = Context.Device.CreatePixelShader(extractPSBlob);

            Blob downsamplePSBlob = ShaderFactory.CompileFromResource("PostProcess.DownsamplePS.hlsl", "main", "PS", "ps_5_0");
            DownsamplePixelShader = Context.Device.CreatePixelShader(downsamplePSBlob);

            Blob upsamplePSBlob = ShaderFactory.CompileFromResource("PostProcess.UpsamplePS.hlsl", "main", "PS", "ps_5_0");
            UpsamplePixelShader = Context.Device.CreatePixelShader(upsamplePSBlob);

            Blob compositePSBlob = ShaderFactory.CompileFromResource("PostProcess.CompositePS.hlsl", "main", "PS", "ps_5_0");
            CompositePixelShader = Context.Device.CreatePixelShader(compositePSBlob);

            Blob fxaaPSBlob = ShaderFactory.CompileFromResource("PostProcess.FxaaPS.hlsl", "main", "PS", "ps_5_0");
            FxaaPixelShader = Context.Device.CreatePixelShader(fxaaPSBlob);

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

            DeferredLightingPixelShader.Dispose();
            ExtractPixelShader.Dispose();
            DownsamplePixelShader.Dispose();
            UpsamplePixelShader.Dispose();
            CompositePixelShader.Dispose();
            FxaaPixelShader.Dispose();

            SamplerState.Dispose();

            PostProcessBuffer.Dispose();
            BlurBuffer.Dispose();

            AdditiveBlendState.Dispose();
            OpaqueBlendState.Dispose();
        }

        public void Reset()
        {
            Buffer?.Dispose();
            Buffer = null;
        }

        public void Setup(ID3D11DepthStencilView depthStencil, SizeI size)
        {
            if (Buffer is null || size != Buffer.Size)
            {
                Buffer?.Dispose();
                Buffer = new PostProcessingBuffer(Context, depthStencil, size);
            }

            Buffer.Initialize();
        }

        public void RenderTo(ID3D11RenderTargetView renderTarget, EnvironmentProfile environment, TimeSpan elapsed)
        {
            if (Buffer is null) throw new InvalidOperationException();

            Context.IASetInputLayout(null);
            Context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
            Context.VSSetShader(VertexShader);
            Context.PSSetSampler(0, SamplerState);


            // 1. Deferred Lighting

            Context.OMSetBlendState(OpaqueBlendState);
            Context.OMSetRenderTargets(Buffer.ResolvedHdrBuffer.RenderTargetView, null);
            Context.RSSetViewport(0, 0, Buffer.Size.Width, Buffer.Size.Height);

            Context.PSSetShader(DeferredLightingPixelShader);
            Context.PSSetShaderResource(0, Buffer.AmbientBuffer.ShaderResourceView);
            Context.PSSetShaderResource(1, Buffer.DirectionalBuffer.ShaderResourceView);
            Context.PSSetShaderResource(2, Buffer.RawShadowDepthBuffer.ShaderResourceView);
            Context.Draw(3, 0);

            Context.PSSetShaderResource(0, null!);
            Context.PSSetShaderResource(1, null!);
            Context.PSSetShaderResource(2, null!);


            // 2. Luminance Extraction

            Context.OMSetRenderTargets(Buffer.LuminanceBuffer.RenderTargetView, null);
            Context.RSSetViewport(0, 0, 1, 1);
            Context.PSSetShader(LuminanceExtractPixelShader);
            Context.PSSetShaderResource(0, Buffer.ResolvedHdrBuffer.ShaderResourceView);
            Context.PSSetShaderResource(1, Buffer.RawShadowDepthBuffer.ShaderResourceView);
            Context.Draw(3, 0);

            Context.PSSetShaderResource(0, null!);
            Context.PSSetShaderResource(1, null!);

            Context.CopyResource(Buffer.StagingTexture, Buffer.LuminanceBuffer.Texture);
            MappedSubresource map = Context.Map(Buffer.StagingTexture, 0, MapMode.Read, MapFlags.None);
            float sceneLuminanceLog;
            unsafe
            {
                sceneLuminanceLog = (float)Unsafe.Read<Half>(map.DataPointer.ToPointer());
            }
            Context.Unmap(Buffer.StagingTexture, 0);

            float targetExposure = environment.Exposure.Key / (float.Exp(sceneLuminanceLog) + 0.0001f);
            targetExposure = float.Clamp(targetExposure, environment.Exposure.Min, environment.Exposure.Max);
            float adaptationSpeed = Exposure < targetExposure ? environment.Exposure.DarkAdaptationSpeed : environment.Exposure.LightAdaptationSpeed;
            Exposure = float.Lerp(Exposure, targetExposure, float.Min(1, (float)elapsed.TotalSeconds * adaptationSpeed));


            float grayIn = environment.Exposure.Key;
            float grayOut = grayIn * environment.ToneMap.MidtoneScale;

            float a = environment.ToneMap.Contrast;
            float d = environment.ToneMap.Shoulder;
            float powMidInA = float.Pow(grayIn, a);
            float powHdrMaxA = float.Pow(environment.ToneMap.MaxLuminance, a);
            float powMidInAD = float.Pow(grayIn, a * d);
            float powHdrMaxAD = float.Pow(environment.ToneMap.MaxLuminance, a * d);

            float denominator = (powHdrMaxAD - powMidInAD) * grayOut;
            if (denominator < 0.00001f && -0.00001f < denominator) denominator = 0.00001f;

            float b = (-powMidInA + powHdrMaxA * grayOut) / denominator;
            float c = (powHdrMaxAD * powMidInA - powHdrMaxA * powMidInAD * grayOut) / denominator;


            PostProcessConstants postProcessConstants = new()
            {
                BloomThreshold = environment.Bloom.Threshold,
                BloomIntensity = environment.Bloom.Intensity,
                BloomScatter = environment.Bloom.Scatter,
                BloomSoftKnee = environment.Bloom.SoftKnee,
                BloomTint = environment.Bloom.Tint.ToLinear(),
                Exposure = Exposure,
                ToneMapA = a,
                ToneMapD = a * d,
                ToneMapB = b,
                ToneMapC = c,
            };
            Context.UpdateSubresource(postProcessConstants, PostProcessBuffer);

            Context.PSSetConstantBuffer(0, PostProcessBuffer);
            Context.PSSetConstantBuffer(1, BlurBuffer);


            // 3. Extract

            Context.OMSetBlendState(OpaqueBlendState);
            Context.OMSetRenderTargets(Buffer.BloomMips[0].RenderTargetView, null);
            Context.RSSetViewport(0, 0, Buffer.BloomMips[0].Size.Width, Buffer.BloomMips[0].Size.Height);

            Context.PSSetShader(ExtractPixelShader);
            Context.PSSetShaderResource(0, Buffer.ResolvedHdrBuffer.ShaderResourceView);
            Context.Draw(3, 0);
            Context.PSSetShaderResource(0, null!);


            // 4. Downsample

            Context.PSSetShader(DownsamplePixelShader);

            for (int i = 0; i < PostProcessingBuffer.BloomMipCount - 1; i++)
            {
                int next = i + 1;
                Context.OMSetRenderTargets(Buffer.BloomMips[next].RenderTargetView, null);
                Context.RSSetViewport(0, 0, Buffer.BloomMips[next].Size.Width, Buffer.BloomMips[next].Size.Height);

                BlurConstants blur = new()
                {
                    TexelSize = new Vector2(1f / Buffer.BloomMips[i].Size.Width, 1f / Buffer.BloomMips[i].Size.Height),
                    BloomScatter = 1,
                };
                Context.UpdateSubresource(blur, BlurBuffer);

                Context.PSSetShaderResource(0, Buffer.BloomMips[i].ShaderResourceView);
                Context.Draw(3, 0);
                Context.PSSetShaderResource(0, null!);
            }


            // 5. Upsample

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


            // 6. Composite

            Context.OMSetBlendState(OpaqueBlendState);
            Context.OMSetRenderTargets(Buffer.LdrBuffer.RenderTargetView, null);
            Context.RSSetViewport(0, 0, Buffer.Size.Width, Buffer.Size.Height);

            Context.PSSetShader(CompositePixelShader);
            Context.PSSetShaderResource(0, Buffer.ResolvedHdrBuffer.ShaderResourceView);
            Context.PSSetShaderResource(1, Buffer.BloomMips[0].ShaderResourceView);
            Context.Draw(3, 0);
            Context.PSSetShaderResource(0, null!);
            Context.PSSetShaderResource(1, null!);


            // 7. FXAA (Antialiasing)

            Context.OMSetRenderTargets(renderTarget, null);
            Context.RSSetViewport(0, 0, Buffer.Size.Width, Buffer.Size.Height);

            Context.PSSetShader(FxaaPixelShader);
            Context.PSSetShaderResource(0, Buffer.LdrBuffer.ShaderResourceView);
            Context.Draw(3, 0);
            Context.PSSetShaderResource(0, null!);
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
