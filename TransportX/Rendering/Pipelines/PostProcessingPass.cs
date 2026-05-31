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
    public class PostProcessingPass : IDisposable
    {
        private readonly List<IDisposable> DXResources = [];

        private readonly RenderContext RenderContext;

        private readonly ID3D11SamplerState SamplerState;
        private readonly ID3D11Buffer PostProcessBuffer;
        private readonly ID3D11Buffer BlurBuffer;

        private readonly GraphicsPipelineState DeferredLightingState;
        private readonly GraphicsPipelineState LuminanceExtractState;
        private readonly GraphicsPipelineState ExtractState;
        private readonly GraphicsPipelineState DownsampleState;
        private readonly GraphicsPipelineState UpsampleState;
        private readonly GraphicsPipelineState CompositeState;
        private readonly GraphicsPipelineState FxaaState;

        private PostProcessingBuffer? Buffer = null;

        private float Exposure = 0;

        public PostProcessingPass(RenderContext renderContext)
        {
            RenderContext = renderContext;


            SamplerDescription samplerDesc = new()
            {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
            };
            SamplerState = RenderContext.DeviceContext.Device.CreateSamplerState(samplerDesc);
            DXResources.Add(SamplerState);

            BufferDescription postProcessBufferDesc = new()
            {
                Usage = ResourceUsage.Default,
                ByteWidth = (uint)PostProcessConstants.Size,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = 0,
            };
            PostProcessBuffer = RenderContext.DeviceContext.Device.CreateBuffer(postProcessBufferDesc);
            DXResources.Add(PostProcessBuffer);

            BufferDescription blurBufferDesc = new()
            {
                Usage = ResourceUsage.Default,
                ByteWidth = (uint)BlurConstants.Size,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = 0,
            };
            BlurBuffer = RenderContext.DeviceContext.Device.CreateBuffer(blurBufferDesc);
            DXResources.Add(BlurBuffer);


            Blob vsBlob = ShaderFactory.CompileFromResource("PostProcess.VS.hlsl", "main", "VS", "vs_5_0");
            ID3D11VertexShader vertexShader = RenderContext.DeviceContext.Device.CreateVertexShader(vsBlob);
            DXResources.Add(vertexShader);

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
            ID3D11BlendState additiveBlendState = RenderContext.DeviceContext.Device.CreateBlendState(additiveBlendDesc);
            DXResources.Add(additiveBlendState);

            BlendDescription opaqueBlendDesc = new();
            opaqueBlendDesc.RenderTarget[0] = new RenderTargetBlendDescription()
            {
                BlendEnable = false,
                RenderTargetWriteMask = ColorWriteEnable.All,
            };
            ID3D11BlendState opaqueBlendState = RenderContext.DeviceContext.Device.CreateBlendState(opaqueBlendDesc);
            DXResources.Add(opaqueBlendState);

            GraphicsPipelineState baseState = new()
            {
                VertexShader = vertexShader,
                PixelShader = null,
                InputLayout = null,

                RasterizerState = null,
                BlendState = null,
                DepthStencilState = null,
            };


            using Blob deferredLightingPSBlob = ShaderFactory.CompileFromResource("PostProcess.DeferredLightingPS.hlsl", "main", "PS", "ps_5_0");
            ID3D11PixelShader deferredLightingPixelShader = RenderContext.DeviceContext.Device.CreatePixelShader(deferredLightingPSBlob);
            DXResources.Add(deferredLightingPixelShader);

            DeferredLightingState = baseState with
            {
                PixelShader = deferredLightingPixelShader,
                BlendState = opaqueBlendState,
            };


            using Blob luminanceExtractPSBlob = ShaderFactory.CompileFromResource("PostProcess.LuminanceExtractPS.hlsl", "main", "PS", "ps_5_0");
            ID3D11PixelShader luminanceExtractPixelShader = RenderContext.DeviceContext.Device.CreatePixelShader(luminanceExtractPSBlob);
            DXResources.Add(luminanceExtractPixelShader);

            LuminanceExtractState = baseState with
            {
                PixelShader = luminanceExtractPixelShader,
                BlendState = opaqueBlendState,
            };


            using Blob extractPSBlob = ShaderFactory.CompileFromResource("PostProcess.ExtractPS.hlsl", "main", "PS", "ps_5_0");
            ID3D11PixelShader extractPixelShader = RenderContext.DeviceContext.Device.CreatePixelShader(extractPSBlob);
            DXResources.Add(extractPixelShader);

            ExtractState = baseState with
            {
                PixelShader = extractPixelShader,
                BlendState = opaqueBlendState,
            };


            using Blob downsamplePSBlob = ShaderFactory.CompileFromResource("PostProcess.DownsamplePS.hlsl", "main", "PS", "ps_5_0");
            ID3D11PixelShader downsamplePixelShader = RenderContext.DeviceContext.Device.CreatePixelShader(downsamplePSBlob);
            DXResources.Add(downsamplePixelShader);

            DownsampleState = baseState with
            {
                PixelShader = downsamplePixelShader,
                BlendState = opaqueBlendState,
            };


            using Blob compositePSBlob = ShaderFactory.CompileFromResource("PostProcess.CompositePS.hlsl", "main", "PS", "ps_5_0");
            ID3D11PixelShader compositePixelShader = RenderContext.DeviceContext.Device.CreatePixelShader(compositePSBlob);
            DXResources.Add(compositePixelShader);

            CompositeState = baseState with
            {
                PixelShader = compositePixelShader,
                BlendState = opaqueBlendState,
            };


            using Blob fxaaPSBlob = ShaderFactory.CompileFromResource("PostProcess.FxaaPS.hlsl", "main", "PS", "ps_5_0");
            ID3D11PixelShader fxaaPixelShader = RenderContext.DeviceContext.Device.CreatePixelShader(fxaaPSBlob);
            DXResources.Add(fxaaPixelShader);

            FxaaState = baseState with
            {
                PixelShader = fxaaPixelShader,
                BlendState = opaqueBlendState,
            };


            using Blob upsamplePSBlob = ShaderFactory.CompileFromResource("PostProcess.UpsamplePS.hlsl", "main", "PS", "ps_5_0");
            ID3D11PixelShader upsamplePixelShader = RenderContext.DeviceContext.Device.CreatePixelShader(upsamplePSBlob);
            DXResources.Add(upsamplePixelShader);

            UpsampleState = baseState with
            {
                PixelShader = upsamplePixelShader,
                BlendState = additiveBlendState,
            };
        }

        public void Dispose()
        {
            Buffer?.Dispose();

            foreach (IDisposable resource in DXResources)
            {
                resource.Dispose();
            }
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
                Buffer = new PostProcessingBuffer(RenderContext.DeviceContext, depthStencil, size);
            }

            Buffer.Initialize();
        }

        public void RenderTo(ID3D11RenderTargetView renderTarget, EnvironmentProfile environment, TimeSpan elapsed)
        {
            if (Buffer is null) throw new InvalidOperationException();

            RenderContext.DeviceContext.PSSetSampler(0, SamplerState);


            // 1. Deferred Lighting

            RenderContext.ApplyState(DeferredLightingState);

            RenderContext.DeviceContext.OMSetRenderTargets(Buffer.ResolvedHdrBuffer.RenderTargetView, null);
            RenderContext.DeviceContext.RSSetViewport(0, 0, Buffer.Size.Width, Buffer.Size.Height);

            RenderContext.DeviceContext.PSSetShaderResource(0, Buffer.AmbientBuffer.ShaderResourceView);
            RenderContext.DeviceContext.PSSetShaderResource(1, Buffer.DirectionalBuffer.ShaderResourceView);
            RenderContext.DeviceContext.PSSetShaderResource(2, Buffer.RawShadowDepthBuffer.ShaderResourceView);
            RenderContext.DeviceContext.Draw(3, 0);
            RenderContext.DeviceContext.PSSetShaderResource(0, null!);
            RenderContext.DeviceContext.PSSetShaderResource(1, null!);
            RenderContext.DeviceContext.PSSetShaderResource(2, null!);


            // 2. Luminance Extraction

            RenderContext.ApplyState(LuminanceExtractState);

            RenderContext.DeviceContext.OMSetRenderTargets(Buffer.LuminanceBuffer.RenderTargetView, null);
            RenderContext.DeviceContext.RSSetViewport(0, 0, 1, 1);
            RenderContext.DeviceContext.PSSetShaderResource(0, Buffer.ResolvedHdrBuffer.ShaderResourceView);
            RenderContext.DeviceContext.PSSetShaderResource(1, Buffer.RawShadowDepthBuffer.ShaderResourceView);
            RenderContext.DeviceContext.Draw(3, 0);
            RenderContext.DeviceContext.PSSetShaderResource(0, null!);
            RenderContext.DeviceContext.PSSetShaderResource(1, null!);

            RenderContext.DeviceContext.CopyResource(Buffer.StagingTexture, Buffer.LuminanceBuffer.Texture);
            MappedSubresource map = RenderContext.DeviceContext.Map(Buffer.StagingTexture, 0, MapMode.Read, MapFlags.None);
            float sceneLuminanceLog;
            unsafe
            {
                sceneLuminanceLog = (float)Unsafe.Read<Half>(map.DataPointer.ToPointer());
            }
            RenderContext.DeviceContext.Unmap(Buffer.StagingTexture, 0);

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
            RenderContext.DeviceContext.UpdateSubresource(postProcessConstants, PostProcessBuffer);

            RenderContext.DeviceContext.PSSetConstantBuffer(0, PostProcessBuffer);
            RenderContext.DeviceContext.PSSetConstantBuffer(1, BlurBuffer);


            // 3. Extract

            RenderContext.ApplyState(ExtractState);

            RenderContext.DeviceContext.OMSetRenderTargets(Buffer.BloomMips[0].RenderTargetView, null);
            RenderContext.DeviceContext.RSSetViewport(0, 0, Buffer.BloomMips[0].Size.Width, Buffer.BloomMips[0].Size.Height);

            RenderContext.DeviceContext.PSSetShaderResource(0, Buffer.ResolvedHdrBuffer.ShaderResourceView);
            RenderContext.DeviceContext.Draw(3, 0);
            RenderContext.DeviceContext.PSSetShaderResource(0, null!);


            // 4. Downsample

            RenderContext.ApplyState(DownsampleState);

            for (int i = 0; i < PostProcessingBuffer.BloomMipCount - 1; i++)
            {
                int next = i + 1;
                RenderContext.DeviceContext.OMSetRenderTargets(Buffer.BloomMips[next].RenderTargetView, null);
                RenderContext.DeviceContext.RSSetViewport(0, 0, Buffer.BloomMips[next].Size.Width, Buffer.BloomMips[next].Size.Height);

                BlurConstants blur = new()
                {
                    TexelSize = new Vector2(1f / Buffer.BloomMips[i].Size.Width, 1f / Buffer.BloomMips[i].Size.Height),
                    BloomScatter = 1,
                };
                RenderContext.DeviceContext.UpdateSubresource(blur, BlurBuffer);

                RenderContext.DeviceContext.PSSetShaderResource(0, Buffer.BloomMips[i].ShaderResourceView);
                RenderContext.DeviceContext.Draw(3, 0);
                RenderContext.DeviceContext.PSSetShaderResource(0, null!);
            }


            // 5. Upsample

            RenderContext.ApplyState(UpsampleState);

            for (int i = PostProcessingBuffer.BloomMipCount - 1; 0 < i; i--)
            {
                int prev = i - 1;
                RenderContext.DeviceContext.OMSetRenderTargets(Buffer.BloomMips[prev].RenderTargetView, null);
                RenderContext.DeviceContext.RSSetViewport(0, 0, Buffer.BloomMips[prev].Size.Width, Buffer.BloomMips[prev].Size.Height);

                BlurConstants blur = new()
                {
                    TexelSize = new Vector2(1f / Buffer.BloomMips[i].Size.Width, 1f / Buffer.BloomMips[i].Size.Height),
                    BloomScatter = postProcessConstants.BloomScatter,
                };
                RenderContext.DeviceContext.UpdateSubresource(blur, BlurBuffer);

                RenderContext.DeviceContext.PSSetShaderResource(0, Buffer.BloomMips[i].ShaderResourceView);
                RenderContext.DeviceContext.Draw(3, 0);
                RenderContext.DeviceContext.PSSetShaderResource(0, null!);
            }


            // 6. Composite

            RenderContext.ApplyState(CompositeState);

            RenderContext.DeviceContext.OMSetRenderTargets(Buffer.LdrBuffer.RenderTargetView, null);
            RenderContext.DeviceContext.RSSetViewport(0, 0, Buffer.Size.Width, Buffer.Size.Height);

            RenderContext.DeviceContext.PSSetShaderResource(0, Buffer.ResolvedHdrBuffer.ShaderResourceView);
            RenderContext.DeviceContext.PSSetShaderResource(1, Buffer.BloomMips[0].ShaderResourceView);
            RenderContext.DeviceContext.Draw(3, 0);
            RenderContext.DeviceContext.PSSetShaderResource(0, null!);
            RenderContext.DeviceContext.PSSetShaderResource(1, null!);


            // 7. FXAA (Antialiasing)

            RenderContext.ApplyState(FxaaState);

            RenderContext.DeviceContext.OMSetRenderTargets(renderTarget, null);
            RenderContext.DeviceContext.RSSetViewport(0, 0, Buffer.Size.Width, Buffer.Size.Height);

            RenderContext.DeviceContext.PSSetShaderResource(0, Buffer.LdrBuffer.ShaderResourceView);
            RenderContext.DeviceContext.Draw(3, 0);
            RenderContext.DeviceContext.PSSetShaderResource(0, null!);
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
