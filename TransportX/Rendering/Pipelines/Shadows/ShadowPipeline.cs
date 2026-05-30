using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D;
using Vortice.Direct3D11;

using TransportX.Rendering.Backend;
using TransportX.Spatial;
using TransportX.Worlds;

namespace TransportX.Rendering.Pipelines.Shadows
{
    public class ShadowPipeline : IDisposable
    {
        private const int CascadeCount = 3;

        private static readonly IReadOnlyList<float> CascadeRadii = [20, 70, 250];


        protected readonly IDXHost DXHost;
        protected readonly ShadowOptions Options;

        protected readonly ShadowMap ShadowMap;

        protected readonly ID3D11VertexShader ShadowVertexShader;
        protected readonly ID3D11InputLayout ShadowInputLayout;
        protected readonly ID3D11Buffer ShadowConstantsBuffer;
        protected readonly ID3D11Buffer CSMSamplingConstantsBuffer;
        protected readonly ID3D11SamplerState ShadowComparisonSampler;
        protected readonly ID3D11RasterizerState ShadowRasterizerState;

        protected readonly Cascade[] Cascades = new Cascade[CascadeCount];

        private uint FrameCount = 0;

        public required ID3D11Buffer InstanceBuffer { protected get; init; }
        public required ID3D11Buffer MaterialBuffer { protected get; init; }

        public ShadowCamera ShadowCamera { get; }

        public ShadowPipeline(IDXHost dxHost, InputElementDescription[] inputElements, ShadowOptions options)
        {
            DXHost = dxHost;
            Options = options;

            ShadowMap = new ShadowMap(DXHost.Device, Options.Resolution, CascadeCount);
            ShadowCamera = new ShadowCamera();

            using Blob shadowVsBlob = ShaderFactory.CompileFromResource("ShadowVS.hlsl", "main", "vs_5_0", "vs_5_0");
            ShadowVertexShader = DXHost.Device.CreateVertexShader(shadowVsBlob);
            ShadowInputLayout = DXHost.Device.CreateInputLayout(inputElements, shadowVsBlob);
            ShadowConstantsBuffer = DXHost.Device.CreateBuffer(new BufferDescription((uint)ShadowConstants.Size, BindFlags.ConstantBuffer, ResourceUsage.Default));
            CSMSamplingConstantsBuffer = DXHost.Device.CreateBuffer(new BufferDescription((uint)CSMSamplingConstants.Size, BindFlags.ConstantBuffer, ResourceUsage.Default));

            SamplerDescription shadowSampDesc = new()
            {
                Filter = Filter.ComparisonMinMagMipLinear,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                ComparisonFunc = ComparisonFunction.LessEqual,
                MinLOD = 0,
                MaxLOD = float.MaxValue,
            };
            ShadowComparisonSampler = DXHost.Device.CreateSamplerState(shadowSampDesc);

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
            ShadowRasterizerState = DXHost.Device.CreateRasterizerState(shadowRasterizerDesc);
        }

        public void Dispose()
        {
            ShadowMap.Dispose();

            ShadowVertexShader.Dispose();
            ShadowInputLayout.Dispose();
            ShadowConstantsBuffer.Dispose();
            CSMSamplingConstantsBuffer.Dispose();
            ShadowComparisonSampler.Dispose();
            ShadowRasterizerState.Dispose();
        }

        public void UpdateCamera(Vector3 lightDirection, WorldBase world)
        {
            if (Options.Resolution <= 0) return;

            ShadowCamera.LocateChunk(world.Camera);

            Vector3 lightDir = Vector3.Normalize(lightDirection);
            Vector3 cameraPosition = world.Camera.WorldPose.Pose.Position;

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

            Cascade lastCascade = Cascades[CascadeCount - 1];
            ShadowCamera.UpdateFromLight(lastCascade.LightView, lastCascade.LightProjection);

            unchecked
            {
                FrameCount++;
            }
        }

        public void Render(IRenderQueue renderQueue, in CameraDrawContext context)
        {
            DXHost.Context.VSSetShader(ShadowVertexShader);
            DXHost.Context.IASetInputLayout(ShadowInputLayout);
            DXHost.Context.RSSetState(ShadowRasterizerState);

            for (int i = 0; i < CascadeCount; i++)
            {
                if (Skip(i)) continue;

                Cascade cascade = Cascades[i];

                DXHost.Context.OMSetRenderTargets((ID3D11RenderTargetView)null!, ShadowMap.DepthStencilViews[i]);
                DXHost.Context.ClearDepthStencilView(ShadowMap.DepthStencilViews[i], DepthStencilClearFlags.Depth, 1, 0);
                DXHost.Context.RSSetViewport(0, 0, ShadowMap.Resolution, ShadowMap.Resolution);

                ShadowCamera.UpdateFromLight(cascade.LightView, cascade.LightProjection);

                ShadowConstants shadowConstants = new()
                {
                    LightViewProjection = Matrix4x4.Transpose(cascade.LightViewProjection),
                };
                DXHost.Context.UpdateSubresource(shadowConstants, ShadowConstantsBuffer);
                DXHost.Context.VSSetConstantBuffer(1, ShadowConstantsBuffer);

                renderQueue.Render(RenderLayer.Normal, new DrawContext()
                {
                    DeviceContext = context.DeviceContext,
                    InstanceBuffer = context.InstanceBuffer,
                    InstanceCount = 0,
                    MaterialBuffer = context.MaterialBuffer,
                });
            }

            renderQueue.Clear();
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

            DXHost.Context.UpdateSubresource(csmConstants, CSMSamplingConstantsBuffer);
            DXHost.Context.PSSetConstantBuffer(3, CSMSamplingConstantsBuffer);

            DXHost.Context.PSSetShaderResource(12, ShadowMap.ShaderResourceView);
            DXHost.Context.PSSetSampler(2, ShadowComparisonSampler);
        }
    }
}
