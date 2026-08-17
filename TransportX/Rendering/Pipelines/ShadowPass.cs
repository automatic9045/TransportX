using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.Mathematics;

using TransportX.Rendering.Backend;
using TransportX.Spatial;
using TransportX.Worlds;
using TransportX.Bodies;

namespace TransportX.Rendering.Pipelines
{
    public class ShadowPass : IRenderPass
    {
        private const int CascadeCount = 3;
        private static readonly IReadOnlyList<float> CascadeRadii = [20, 70, 250];


        protected readonly RenderResourceSet Resources;
        protected readonly ShadowOptions Options;

        protected readonly GraphicsPipelineState PipelineState;
        protected readonly ID3D11SamplerState ComparisonSamplerState;
        protected readonly ID3D11Buffer ShadowBuffer;
        protected readonly ID3D11Buffer SamplingBuffer;

        protected readonly ShadowMap ShadowMap;
        protected readonly ShadowCamera ShadowCamera;

        protected readonly RenderQueue RenderQueue = new();
        protected readonly ShadowCascade[] Cascades = new ShadowCascade[CascadeCount];
        protected readonly BoundingSphere[] CascadeSpheres = new BoundingSphere[CascadeCount];

        public ShadowPass(RenderResourceSet resources, ShadowOptions options)
        {
            Resources = resources;
            Options = options;

            ID3D11Device device = Resources.Context.DeviceContext.Device;


            using Blob shadowVsBlob = ShaderFactory.CompileFromResource("ShadowVS.hlsl", "main", "vs_5_0", "vs_5_0");
            ID3D11VertexShader vertexShader = device.CreateVertexShader(shadowVsBlob);
            ID3D11InputLayout inputLayout = device.CreateInputLayout(IRenderer.DefaultInputElements.ToArray(), shadowVsBlob);

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
            ID3D11RasterizerState rasterizerState = device.CreateRasterizerState(shadowRasterizerDesc);

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
            ComparisonSamplerState = device.CreateSamplerState(samplerDesc);

            BufferDescription shadowBufferDesc = new((uint)ShadowConstants.Size, BindFlags.ConstantBuffer, ResourceUsage.Default);
            ShadowBuffer = device.CreateBuffer(shadowBufferDesc);

            BufferDescription samplingBufferDesc = new((uint)CSMSamplingConstants.Size, BindFlags.ConstantBuffer, ResourceUsage.Default);
            SamplingBuffer = device.CreateBuffer(samplingBufferDesc);


            ShadowMap = new ShadowMap(device, Options.Resolution, CascadeCount);
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

        public void Execute(in RenderPassContext context, WorldBase world)
        {
            if (Options.Resolution <= 0) return;

            UpdateCamera(world.DirectionalLight.Direction, context.ViewContext);
            Render(world.Chunks, world.Bodies);
            Bind();
        }

        private void UpdateCamera(Vector3 lightDirection, in ViewContext viewContext)
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

                CascadeSpheres[i] = new BoundingSphere(sphereCenterWorld, sphereRadius);

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

        private void Render(ChunkCollection chunks, IReadOnlyList<RigidBody> bodies)
        {
            ID3D11DeviceContext deviceContext = Resources.Context.DeviceContext;

            deviceContext.VSSetConstantBuffer(1, ShadowBuffer);
            Resources.Context.ApplyState(PipelineState);

            for (int i = 0; i < CascadeCount; i++)
            {
                ShadowCascade cascade = Cascades[i];

                deviceContext.RSSetViewport(0, 0, Options.Resolution, Options.Resolution);
                deviceContext.OMSetRenderTargets((ID3D11RenderTargetView)null!, ShadowMap.DepthStencilViews[i]);
                deviceContext.ClearDepthStencilView(ShadowMap.DepthStencilViews[i], DepthStencilClearFlags.Depth, 1, 0);

                ViewContext shadowViewContext = ShadowCamera.CreateViewContext(cascade.LightView, cascade.LightProjection);

                ShadowConstants shadowConstants = new()
                {
                    LightViewProjection = Matrix4x4.Transpose(cascade.LightViewProjection),
                };
                deviceContext.UpdateSubresource(shadowConstants, ShadowBuffer);

                SphereCullingVolume culler = new(CascadeSpheres[i]);

                RenderQueue.SubmitChunks(deviceContext, shadowViewContext, culler, chunks, RenderLayer.Normal, Options.DrawChunkCount);
                RenderQueue.SubmitBodies(deviceContext, shadowViewContext, culler, bodies, RenderLayer.Normal);

                RenderQueue.Render(new DrawContext()
                {
                    DeviceContext = deviceContext,
                    InstanceBuffer = Resources.InstanceBuffer,
                    InstanceCount = 0,
                    MaterialBuffer = Resources.MaterialBuffer,
                });
                RenderQueue.Clear();
            }

            deviceContext.OMSetRenderTargets((ID3D11RenderTargetView)null!, null);
        }

        private void Bind()
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

            ID3D11DeviceContext deviceContext = Resources.Context.DeviceContext;

            deviceContext.UpdateSubresource(csmConstants, SamplingBuffer);
            deviceContext.PSSetConstantBuffer(3, SamplingBuffer);

            deviceContext.PSSetShaderResource(12, ShadowMap.ShaderResourceView);
            deviceContext.PSSetSampler(2, ComparisonSamplerState);
        }
    }
}
