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
using TransportX.Cameras;

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

        private uint FrameCount = 0;

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

        public void UpdateCamera(Vector3 lightDirection, Camera camera)
        {
            if (Options.Resolution <= 0) return;

            ShadowCamera.LocateChunk(camera.WorldPose.Chunk);

            Vector3 lightDir = Vector3.Normalize(lightDirection);
            Vector3 cameraPosition = camera.WorldPose.Pose.Position;

            for (int i = 0; i < CascadeCount; i++)
            {
                if (Skip(i)) continue;

                float radius = CascadeRadii[i];
                float texelsPerUnit = Options.Resolution / (radius * 2);

                Vector3 upVector = Vector3.UnitY;
                if (0.999f < float.Abs(Vector3.Dot(lightDir, Vector3.UnitY)))
                {
                    upVector = Vector3.UnitZ;
                }
                Vector3 zAxis = lightDir;
                Vector3 xAxis = Vector3.Normalize(Vector3.Cross(upVector, zAxis));
                Vector3 yAxis = Vector3.Cross(zAxis, xAxis);

                Matrix4x4 baseLightView = new(
                    xAxis.X, yAxis.X, zAxis.X, 0,
                    xAxis.Y, yAxis.Y, zAxis.Y, 0,
                    xAxis.Z, yAxis.Z, zAxis.Z, 0,
                    0, 0, 0, 1
                );

                Vector3 centerLightSpace = Vector3.Transform(cameraPosition, baseLightView);
                centerLightSpace.X = float.Floor(centerLightSpace.X * texelsPerUnit) / texelsPerUnit;
                centerLightSpace.Y = float.Floor(centerLightSpace.Y * texelsPerUnit) / texelsPerUnit;

                Matrix4x4.Invert(baseLightView, out Matrix4x4 inverseBaseLightView);
                Vector3 sphereCenter = Vector3.Transform(centerLightSpace, inverseBaseLightView);

                float zPullback = (Options.DrawChunkCount + 1) * Chunk.Size;
                Vector3 lightPos = sphereCenter - (lightDir * zPullback);

                Matrix4x4 lightView = new(
                    xAxis.X, yAxis.X, zAxis.X, 0,
                    xAxis.Y, yAxis.Y, zAxis.Y, 0,
                    xAxis.Z, yAxis.Z, zAxis.Z, 0,
                    -Vector3.Dot(xAxis, lightPos), -Vector3.Dot(yAxis, lightPos), -Vector3.Dot(zAxis, lightPos), 1
                );

                Matrix4x4 lightProjection = Matrix4x4.CreateOrthographicOffCenterLeftHanded(-radius, radius, -radius, radius, 0, zPullback + radius);

                Cascades[i] = new()
                {
                    LightView = lightView,
                    LightProjection = lightProjection,
                    LightViewProjection = lightView * lightProjection,
                    SplitDepth = radius,
                };
            }

            ShadowCascade lastCascade = Cascades[CascadeCount - 1];
            ShadowCamera.UpdateFromLight(lastCascade.LightView, lastCascade.LightProjection);

            unchecked
            {
                FrameCount++;
            }
        }

        public void Render(ChunkCollection chunks, IReadOnlyList<RigidBody> bodies)
        {
            RenderContext.ApplyState(PipelineState);

            for (int i = 0; i < CascadeCount; i++)
            {
                if (Skip(i)) continue;

                ShadowCascade cascade = Cascades[i];

                RenderContext.DeviceContext.OMSetRenderTargets((ID3D11RenderTargetView)null!, ShadowMap.DepthStencilViews[i]);
                RenderContext.DeviceContext.ClearDepthStencilView(ShadowMap.DepthStencilViews[i], DepthStencilClearFlags.Depth, 1, 0);
                RenderContext.DeviceContext.RSSetViewport(0, 0, ShadowMap.Resolution, ShadowMap.Resolution);

                ShadowCamera.UpdateFromLight(cascade.LightView, cascade.LightProjection);

                ShadowConstants shadowConstants = new()
                {
                    LightViewProjection = Matrix4x4.Transpose(cascade.LightViewProjection),
                };
                RenderContext.DeviceContext.UpdateSubresource(shadowConstants, ShadowBuffer);
                RenderContext.DeviceContext.VSSetConstantBuffer(1, ShadowBuffer);

                RenderQueue.SubmitChunks(RenderContext.DeviceContext, ShadowCamera, chunks, RenderLayer.Normal, Options.DrawChunkCount);
                RenderQueue.SubmitBodies(RenderContext.DeviceContext, ShadowCamera, bodies, RenderLayer.Normal);

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

        protected bool Skip(int cascadeIndex)
        {
            return false;
            //int updateSpan = 1 << cascadeIndex;
            //return FrameCount % updateSpan != 0;
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
