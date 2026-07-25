using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D;
using Vortice.Direct3D11;

using TransportX.Bodies;
using TransportX.Rendering.Backend;
using TransportX.Spatial;

namespace TransportX.Rendering.Pipelines
{
    public class ShadowPass : IDisposable
    {
        private const int CascadeCount = 3;
        private static readonly IReadOnlyList<float> CascadeRadii = [20, 70, 250];


        protected readonly RenderContext RenderContext;
        protected readonly ShadowOptions Options;

        protected readonly GraphicsPipelineState PipelineState;
        protected readonly ID3D11SamplerState ComparisonSamplerState;
        protected readonly ID3D11Buffer ShadowBuffer;
        protected readonly ID3D11Buffer SamplingBuffer;

        protected readonly ShadowMap ShadowMap;
        protected readonly ShadowCamera ShadowCamera;

        protected readonly RenderQueue RenderQueue = new();
        protected readonly ShadowCascade[] Cascades = new ShadowCascade[CascadeCount];

        public required ID3D11Buffer InstanceBuffer { protected get; init; }
        public required ID3D11Buffer MaterialBuffer { protected get; init; }

        public ShadowPass(RenderContext renderContext, InputElementDescription[] inputElements, ShadowOptions options)
        {
            RenderContext = renderContext;
            Options = options;


            using Blob shadowVsBlob = ShaderFactory.CompileFromResource("ShadowVS.hlsl", "main", "vs_5_0", "vs_5_0");
            ID3D11VertexShader vertexShader = RenderContext.DeviceContext.Device.CreateVertexShader(shadowVsBlob);
            ID3D11InputLayout inputLayout = RenderContext.DeviceContext.Device.CreateInputLayout(inputElements, shadowVsBlob);

            RasterizerDescription shadowRasterizerDesc = new()
            {
                AntialiasedLineEnable = false,
                CullMode = CullMode.None,
                DepthBias = 0,
                DepthBiasClamp = 0,
                DepthClipEnable = true,
                FillMode = FillMode.Solid,
                FrontCounterClockwise = false,
                MultisampleEnable = false,
                ScissorEnable = false,
                SlopeScaledDepthBias = 0,
            };
            ID3D11RasterizerState rasterizerState = RenderContext.DeviceContext.Device.CreateRasterizerState(shadowRasterizerDesc);

            PipelineState = new GraphicsPipelineState()
            {
                VertexShader = vertexShader,
                PixelShader = null,
                InputLayout = inputLayout,

                RasterizerState = rasterizerState,
                BlendState = null,
                DepthStencilState = null,
            };


            SamplerDescription samplerDesc = new()
            {
                Filter = Filter.ComparisonMinMagMipLinear,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                ComparisonFunc = ComparisonFunction.LessEqual,
                MinLOD = 0,
                MaxLOD = float.MaxValue,
            };
            ComparisonSamplerState = RenderContext.DeviceContext.Device.CreateSamplerState(samplerDesc);

            BufferDescription shadowBufferDesc = new((uint)ShadowConstants.Size, BindFlags.ConstantBuffer, ResourceUsage.Default);
            ShadowBuffer = RenderContext.DeviceContext.Device.CreateBuffer(shadowBufferDesc);

            BufferDescription samplingBufferDesc = new((uint)CSMSamplingConstants.Size, BindFlags.ConstantBuffer, ResourceUsage.Default);
            SamplingBuffer = RenderContext.DeviceContext.Device.CreateBuffer(samplingBufferDesc);


            ShadowMap = new ShadowMap(RenderContext.DeviceContext.Device, Options.Resolution, CascadeCount);
            ShadowCamera = new ShadowCamera();
        }

        public void Dispose()
        {
            PipelineState.Dispose();
            ComparisonSamplerState.Dispose();
            ShadowBuffer.Dispose();
            SamplingBuffer.Dispose();

            ShadowMap.Dispose();
        }

        public void UpdateCamera(Vector3 lightDirection, in ViewContext viewContext)
        {
            if (Options.Resolution <= 0) return;

            ShadowCamera.LocateChunk(viewContext.WorldPose.Chunk);

            Matrix4x4.Invert(viewContext.View, out Matrix4x4 viewInverse);

            float tanHalfFovX = 1.0f / viewContext.Projection.M11;
            float tanHalfFovY = 1.0f / viewContext.Projection.M22;

            for (int i = 0; i < CascadeCount; i++)
            {
                float nearSplit = i == 0 ? 0.1f : CascadeRadii[i - 1];
                float farSplit = CascadeRadii[i];

                float sliceCenterZ = 0.5f * (nearSplit + farSplit);
                Vector3 sliceCenterView = new(0, 0, sliceCenterZ);

                Vector3 farCornerView = new(farSplit * tanHalfFovX, farSplit * tanHalfFovY, farSplit);
                float sphereRadiusSquared = Vector3.DistanceSquared(sliceCenterView, farCornerView);
                float sphereRadius = float.Sqrt(sphereRadiusSquared);

                Vector3 sphereCenterWorld = Vector3.Transform(sliceCenterView, viewInverse);

                float pullback = sphereRadius + (Options.DrawChunkCount + 1) * Chunk.Size;
                Vector3 upVector = 0.99f < float.Abs(lightDirection.Y) ? Vector3.UnitZ : Vector3.UnitY;
                Matrix4x4 shadowView = Matrix4x4.CreateLookAtLeftHanded(-lightDirection * pullback, Vector3.Zero, upVector);

                Vector3 centerInLightSpace = Vector3.Transform(sphereCenterWorld, shadowView);

                float minX = centerInLightSpace.X - sphereRadius;
                float maxX = centerInLightSpace.X + sphereRadius;
                float minY = centerInLightSpace.Y - sphereRadius;
                float maxY = centerInLightSpace.Y + sphereRadius;

                if (0 < Options.Resolution)
                {
                    float worldUnitsPerTexel = sphereRadius * 2 / Options.Resolution;
                    minX = float.Floor(minX / worldUnitsPerTexel) * worldUnitsPerTexel;
                    maxX = minX + sphereRadius * 2;
                    minY = float.Floor(minY / worldUnitsPerTexel) * worldUnitsPerTexel;
                    maxY = minY + sphereRadius * 2;
                }

                float minZ = 0;
                float maxZ = centerInLightSpace.Z + sphereRadius;

                Matrix4x4 lightProjection = Matrix4x4.CreateOrthographicOffCenterLeftHanded(minX, maxX, minY, maxY, minZ, maxZ);

                Cascades[i] = new ShadowCascade()
                {
                    LightView = shadowView,
                    LightProjection = lightProjection,
                    LightViewProjection = shadowView * lightProjection,
                    SplitDepth = farSplit,
                };
            }
        }

        public void Render(ChunkCollection chunks, IReadOnlyList<RigidBody> bodies)
        {
            if (Options.Resolution <= 0) return;

            RenderContext.DeviceContext.VSSetConstantBuffer(1, ShadowBuffer);
            RenderContext.ApplyState(PipelineState);

            for (int i = 0; i < CascadeCount; i++)
            {
                ShadowCascade cascade = Cascades[i];

                RenderContext.DeviceContext.RSSetViewport(0, 0, Options.Resolution, Options.Resolution);
                RenderContext.DeviceContext.OMSetRenderTargets((ID3D11RenderTargetView)null!, ShadowMap.DepthStencilViews[i]);
                RenderContext.DeviceContext.ClearDepthStencilView(ShadowMap.DepthStencilViews[i], DepthStencilClearFlags.Depth, 1, 0);

                ViewContext shadowViewContext = ShadowCamera.CreateViewContext(cascade.LightView, cascade.LightProjection);

                ShadowConstants shadowConstants = new()
                {
                    LightViewProjection = Matrix4x4.Transpose(cascade.LightViewProjection),
                };
                RenderContext.DeviceContext.UpdateSubresource(shadowConstants, ShadowBuffer);

                RenderQueue.SubmitChunks(RenderContext.DeviceContext, shadowViewContext, chunks, RenderLayer.Normal, Options.DrawChunkCount);
                RenderQueue.SubmitBodies(RenderContext.DeviceContext, shadowViewContext, bodies, RenderLayer.Normal);

                RenderQueue.Render(new DrawContext()
                {
                    DeviceContext = RenderContext.DeviceContext,
                    InstanceBuffer = InstanceBuffer,
                    InstanceCount = 0,
                    MaterialBuffer = MaterialBuffer,
                });
                RenderQueue.Clear();
            }
        }

        public void Bind()
        {
            CSMSamplingConstants csmConstants;
            if (Options.Resolution <= 0)
            {
                csmConstants = new()
                {
                    LightViewProjection0 = Matrix4x4.Identity,
                    LightViewProjection1 = Matrix4x4.Identity,
                    LightViewProjection2 = Matrix4x4.Identity,
                    LightViewProjection3 = Matrix4x4.Identity,
                    SplitDepths = new Vector4(CascadeRadii[0], CascadeRadii[1], CascadeRadii[2], 0),
                    Resolution = 1,
                    ZPullback = 0,
                };
            }
            else
            {
                csmConstants = new()
                {
                    LightViewProjection0 = Matrix4x4.Transpose(Cascades[0].LightViewProjection),
                    LightViewProjection1 = Matrix4x4.Transpose(Cascades[1].LightViewProjection),
                    LightViewProjection2 = Matrix4x4.Transpose(Cascades[2].LightViewProjection),
                    LightViewProjection3 = Matrix4x4.Identity,
                    SplitDepths = new Vector4(Cascades[0].SplitDepth, Cascades[1].SplitDepth, Cascades[2].SplitDepth, 0),
                    Resolution = Options.Resolution,
                    ZPullback = (Options.DrawChunkCount + 1) * Chunk.Size,
                };
            }

            RenderContext.DeviceContext.UpdateSubresource(csmConstants, SamplingBuffer);
            RenderContext.DeviceContext.PSSetConstantBuffer(3, SamplingBuffer);

            RenderContext.DeviceContext.PSSetShaderResource(12, ShadowMap.ShaderResourceView);
            RenderContext.DeviceContext.PSSetSampler(2, ComparisonSamplerState);
        }
    }
}
