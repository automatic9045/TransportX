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

using TransportX.Rendering.Backend;
using TransportX.Worlds;
using TransportX.Environment;

namespace TransportX.Rendering.Pipelines
{
    public class PostProcessPass : IRenderPass
    {
        private readonly List<IDisposable> DXResources = [];

        private readonly RenderResourceSet Resources;

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

        public PostProcessPass(RenderResourceSet resources)
        {
            Resources = resources;
            ID3D11Device device = Resources.Context.DeviceContext.Device;


            SamplerDescription samplerDesc = new()
            {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
            };
            SamplerState = device.CreateSamplerState(samplerDesc);
            DXResources.Add(SamplerState);

            BufferDescription postProcessBufferDesc = new()
            {
                Usage = ResourceUsage.Default,
                ByteWidth = (uint)PostProcessConstants.Size,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = 0,
            };
            PostProcessBuffer = device.CreateBuffer(postProcessBufferDesc);
            DXResources.Add(PostProcessBuffer);

            BufferDescription blurBufferDesc = new()
            {
                Usage = ResourceUsage.Default,
                ByteWidth = (uint)BlurConstants.Size,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = 0,
            };
            BlurBuffer = device.CreateBuffer(blurBufferDesc);
            DXResources.Add(BlurBuffer);


            Blob vsBlob = ShaderFactory.CompileFromResource("PostProcess.VS.hlsl", "main", "VS", "vs_5_0");
            ID3D11VertexShader vertexShader = device.CreateVertexShader(vsBlob);
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
            ID3D11BlendState additiveBlendState = device.CreateBlendState(additiveBlendDesc);
            DXResources.Add(additiveBlendState);

            BlendDescription opaqueBlendDesc = new();
            opaqueBlendDesc.RenderTarget[0] = new RenderTargetBlendDescription()
            {
                BlendEnable = false,
                RenderTargetWriteMask = ColorWriteEnable.All,
            };
            ID3D11BlendState opaqueBlendState = device.CreateBlendState(opaqueBlendDesc);
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
            ID3D11PixelShader deferredLightingPixelShader = device.CreatePixelShader(deferredLightingPSBlob);
            DXResources.Add(deferredLightingPixelShader);

            DeferredLightingState = baseState with
            {
                PixelShader = deferredLightingPixelShader,
                BlendState = opaqueBlendState,
            };


            using Blob luminanceExtractPSBlob = ShaderFactory.CompileFromResource("PostProcess.LuminanceExtractPS.hlsl", "main", "PS", "ps_5_0");
            ID3D11PixelShader luminanceExtractPixelShader = device.CreatePixelShader(luminanceExtractPSBlob);
            DXResources.Add(luminanceExtractPixelShader);

            LuminanceExtractState = baseState with
            {
                PixelShader = luminanceExtractPixelShader,
                BlendState = opaqueBlendState,
            };


            using Blob extractPSBlob = ShaderFactory.CompileFromResource("PostProcess.ExtractPS.hlsl", "main", "PS", "ps_5_0");
            ID3D11PixelShader extractPixelShader = device.CreatePixelShader(extractPSBlob);
            DXResources.Add(extractPixelShader);

            ExtractState = baseState with
            {
                PixelShader = extractPixelShader,
                BlendState = opaqueBlendState,
            };


            using Blob downsamplePSBlob = ShaderFactory.CompileFromResource("PostProcess.DownsamplePS.hlsl", "main", "PS", "ps_5_0");
            ID3D11PixelShader downsamplePixelShader = device.CreatePixelShader(downsamplePSBlob);
            DXResources.Add(downsamplePixelShader);

            DownsampleState = baseState with
            {
                PixelShader = downsamplePixelShader,
                BlendState = opaqueBlendState,
            };


            using Blob compositePSBlob = ShaderFactory.CompileFromResource("PostProcess.CompositePS.hlsl", "main", "PS", "ps_5_0");
            ID3D11PixelShader compositePixelShader = device.CreatePixelShader(compositePSBlob);
            DXResources.Add(compositePixelShader);

            CompositeState = baseState with
            {
                PixelShader = compositePixelShader,
                BlendState = opaqueBlendState,
            };


            using Blob fxaaPSBlob = ShaderFactory.CompileFromResource("PostProcess.FxaaPS.hlsl", "main", "PS", "ps_5_0");
            ID3D11PixelShader fxaaPixelShader = device.CreatePixelShader(fxaaPSBlob);
            DXResources.Add(fxaaPixelShader);

            FxaaState = baseState with
            {
                PixelShader = fxaaPixelShader,
                BlendState = opaqueBlendState,
            };


            using Blob upsamplePSBlob = ShaderFactory.CompileFromResource("PostProcess.UpsamplePS.hlsl", "main", "PS", "ps_5_0");
            ID3D11PixelShader upsamplePixelShader = device.CreatePixelShader(upsamplePSBlob);
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
                Buffer = new PostProcessingBuffer(Resources.Context.DeviceContext, depthStencil, size);
            }

            Buffer.Initialize();
        }

        public void Execute(in RenderPassContext context, WorldBase world)
        {
            if (Buffer is null) throw new InvalidOperationException();

            ID3D11DeviceContext deviceContext = Resources.Context.DeviceContext;
            deviceContext.PSSetSampler(0, SamplerState);


            // 1. Deferred Lighting

            Resources.Context.ApplyState(DeferredLightingState);

            deviceContext.OMSetRenderTargets(Buffer.ResolvedHdrBuffer.RenderTargetView, null);
            deviceContext.RSSetViewport(0, 0, Buffer.Size.Width, Buffer.Size.Height);

            deviceContext.PSSetShaderResource(0, Buffer.AmbientBuffer.ShaderResourceView);
            deviceContext.PSSetShaderResource(1, Buffer.DirectionalBuffer.ShaderResourceView);
            deviceContext.PSSetShaderResource(2, Buffer.RawShadowDepthBuffer.ShaderResourceView);
            deviceContext.Draw(3, 0);
            deviceContext.PSSetShaderResource(0, null!);
            deviceContext.PSSetShaderResource(1, null!);
            deviceContext.PSSetShaderResource(2, null!);


            // 2. Luminance Extraction

            Resources.Context.ApplyState(LuminanceExtractState);

            deviceContext.OMSetRenderTargets(Buffer.LuminanceBuffer.RenderTargetView, null);
            deviceContext.RSSetViewport(0, 0, 1, 1);
            deviceContext.PSSetShaderResource(0, Buffer.ResolvedHdrBuffer.ShaderResourceView);
            deviceContext.PSSetShaderResource(1, Buffer.RawShadowDepthBuffer.ShaderResourceView);
            deviceContext.Draw(3, 0);
            deviceContext.PSSetShaderResource(0, null!);
            deviceContext.PSSetShaderResource(1, null!);

            deviceContext.CopyResource(Buffer.StagingTexture, Buffer.LuminanceBuffer.Texture);
            MappedSubresource map = deviceContext.Map(Buffer.StagingTexture, 0, MapMode.Read, MapFlags.None);
            float sceneLuminanceLog;
            unsafe
            {
                sceneLuminanceLog = (float)Unsafe.Read<Half>(map.DataPointer.ToPointer());
            }
            deviceContext.Unmap(Buffer.StagingTexture, 0);

            EnvironmentProfile environment = world.DefaultEnvironment;

            float targetExposure = environment.Exposure.Key / (float.Exp(sceneLuminanceLog) + 0.0001f);
            targetExposure = float.Clamp(targetExposure, environment.Exposure.Min, environment.Exposure.Max);
            float adaptationSpeed = Exposure < targetExposure ? environment.Exposure.DarkAdaptationSpeed : environment.Exposure.LightAdaptationSpeed;
            Exposure = float.Lerp(Exposure, targetExposure, float.Min(1, (float)context.Elapsed.TotalSeconds * adaptationSpeed));


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
            deviceContext.UpdateSubresource(postProcessConstants, PostProcessBuffer);

            deviceContext.PSSetConstantBuffer(0, PostProcessBuffer);
            deviceContext.PSSetConstantBuffer(1, BlurBuffer);


            // 3. Extract

            Resources.Context.ApplyState(ExtractState);

            deviceContext.OMSetRenderTargets(Buffer.BloomMips[0].RenderTargetView, null);
            deviceContext.RSSetViewport(0, 0, Buffer.BloomMips[0].Size.Width, Buffer.BloomMips[0].Size.Height);

            deviceContext.PSSetShaderResource(0, Buffer.ResolvedHdrBuffer.ShaderResourceView);
            deviceContext.Draw(3, 0);
            deviceContext.PSSetShaderResource(0, null!);


            // 4. Downsample

            Resources.Context.ApplyState(DownsampleState);

            for (int i = 0; i < PostProcessingBuffer.BloomMipCount - 1; i++)
            {
                int next = i + 1;
                deviceContext.OMSetRenderTargets(Buffer.BloomMips[next].RenderTargetView, null);
                deviceContext.RSSetViewport(0, 0, Buffer.BloomMips[next].Size.Width, Buffer.BloomMips[next].Size.Height);

                BlurConstants blur = new()
                {
                    TexelSize = new Vector2(1f / Buffer.BloomMips[i].Size.Width, 1f / Buffer.BloomMips[i].Size.Height),
                    BloomScatter = 1,
                };
                deviceContext.UpdateSubresource(blur, BlurBuffer);

                deviceContext.PSSetShaderResource(0, Buffer.BloomMips[i].ShaderResourceView);
                deviceContext.Draw(3, 0);
                deviceContext.PSSetShaderResource(0, null!);
            }


            // 5. Upsample

            Resources.Context.ApplyState(UpsampleState);

            for (int i = PostProcessingBuffer.BloomMipCount - 1; 0 < i; i--)
            {
                int prev = i - 1;
                deviceContext.OMSetRenderTargets(Buffer.BloomMips[prev].RenderTargetView, null);
                deviceContext.RSSetViewport(0, 0, Buffer.BloomMips[prev].Size.Width, Buffer.BloomMips[prev].Size.Height);

                BlurConstants blur = new()
                {
                    TexelSize = new Vector2(1f / Buffer.BloomMips[i].Size.Width, 1f / Buffer.BloomMips[i].Size.Height),
                    BloomScatter = postProcessConstants.BloomScatter,
                };
                deviceContext.UpdateSubresource(blur, BlurBuffer);

                deviceContext.PSSetShaderResource(0, Buffer.BloomMips[i].ShaderResourceView);
                deviceContext.Draw(3, 0);
                deviceContext.PSSetShaderResource(0, null!);
            }


            // 6. Composite

            Resources.Context.ApplyState(CompositeState);

            deviceContext.OMSetRenderTargets(Buffer.LdrBuffer.RenderTargetView, null);
            deviceContext.RSSetViewport(0, 0, Buffer.Size.Width, Buffer.Size.Height);

            deviceContext.PSSetShaderResource(0, Buffer.ResolvedHdrBuffer.ShaderResourceView);
            deviceContext.PSSetShaderResource(1, Buffer.BloomMips[0].ShaderResourceView);
            deviceContext.Draw(3, 0);
            deviceContext.PSSetShaderResource(0, null!);
            deviceContext.PSSetShaderResource(1, null!);


            // 7. FXAA (Antialiasing)

            Resources.Context.ApplyState(FxaaState);

            deviceContext.OMSetRenderTargets(context.Surface.RenderTarget!, null);
            deviceContext.RSSetViewport(0, 0, Buffer.Size.Width, Buffer.Size.Height);

            deviceContext.PSSetShaderResource(0, Buffer.LdrBuffer.ShaderResourceView);
            deviceContext.Draw(3, 0);
            deviceContext.PSSetShaderResource(0, null!);
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
