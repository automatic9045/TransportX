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

namespace TransportX.Rendering.Pipelines
{
    public class OpaquePass : IRenderPass
    {
        protected readonly RenderResourceSet Resources;

        protected readonly GraphicsPipelineState PipelineState;
        protected readonly GraphicsPipelineState ReflectPipelineState;
        protected readonly ID3D11SamplerState TextureSamplerState;

        protected readonly RenderQueue RenderQueue = new();

        public bool IsReflect { get; set; } = false;

        public OpaquePass(RenderResourceSet resources)
        {
            Resources = resources;


            Blob vsBlob = ShaderFactory.CompileFromResource("VS.hlsl", "main", "VS", "vs_5_0");
            ID3D11VertexShader vertexShader = Resources.Context.DeviceContext.Device.CreateVertexShader(vsBlob);

            Blob psBlob = ShaderFactory.CompileFromResource("PS.hlsl", "main", "PS", "ps_5_0");
            ID3D11PixelShader pixelShader = Resources.Context.DeviceContext.Device.CreatePixelShader(psBlob);

            ID3D11InputLayout inputLayout = Resources.Context.DeviceContext.Device.CreateInputLayout(IRenderer.DefaultInputElements.ToArray(), vsBlob);

            RasterizerDescription rasterizerDesc = new()
            {
                AntialiasedLineEnable = false,
                CullMode = CullMode.Back,
                DepthBias = 0,
                DepthBiasClamp = 0,
                DepthClipEnable = true,
                FillMode = FillMode.Solid,
                FrontCounterClockwise = false,
                MultisampleEnable = false,
                ScissorEnable = false,
                SlopeScaledDepthBias = 0,
            };
            ID3D11RasterizerState rasterizerState = Resources.Context.DeviceContext.Device.CreateRasterizerState(rasterizerDesc);

            rasterizerDesc.FrontCounterClockwise = true;
            ID3D11RasterizerState reflectRasterizerState = Resources.Context.DeviceContext.Device.CreateRasterizerState(rasterizerDesc);

            BlendDescription blendDesc = new()
            {
                AlphaToCoverageEnable = true,
                IndependentBlendEnable = true,
            };
            blendDesc.RenderTarget[0] = new RenderTargetBlendDescription()
            {
                BlendEnable = true,
                SourceBlend = Blend.SourceAlpha,
                DestinationBlend = Blend.InverseSourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceBlendAlpha = Blend.One,
                DestinationBlendAlpha = Blend.Zero,
                BlendOperationAlpha = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteEnable.All,
            };
            blendDesc.RenderTarget[1] = new RenderTargetBlendDescription()
            {
                BlendEnable = false,
                RenderTargetWriteMask = ColorWriteEnable.All,
            };
            blendDesc.RenderTarget[2] = new RenderTargetBlendDescription()
            {
                BlendEnable = false,
                RenderTargetWriteMask = ColorWriteEnable.All,
            };
            blendDesc.RenderTarget[3] = new RenderTargetBlendDescription()
            {
                BlendEnable = false,
                RenderTargetWriteMask = ColorWriteEnable.All,
            };
            ID3D11BlendState blendState = Resources.Context.DeviceContext.Device.CreateBlendState(blendDesc);

            PipelineState = new GraphicsPipelineState()
            {
                VertexShader = vertexShader,
                PixelShader = pixelShader,
                InputLayout = inputLayout,
                RasterizerState = rasterizerState,
                BlendState = blendState,
                DepthStencilState = null,
                PrimitiveTopology = PrimitiveTopology.TriangleList,
            };
            ReflectPipelineState = PipelineState with
            {
                RasterizerState = reflectRasterizerState,
            };


            SamplerDescription samplerDesc = new()
            {
                Filter = Filter.Anisotropic,
                MaxAnisotropy = 16,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                ComparisonFunc = ComparisonFunction.Never,
                MinLOD = 0,
                MaxLOD = float.MaxValue,
            };
            TextureSamplerState = Resources.Context.DeviceContext.Device.CreateSamplerState(samplerDesc);
        }

        public void Dispose()
        {
            PipelineState.Dispose();
            TextureSamplerState.Dispose();
        }

        public void Execute(in RenderPassContext context, WorldBase world)
        {
            Resources.Context.DeviceContext.RSSetViewport(0, 0, context.ViewportSize.Width, context.ViewportSize.Height);
            Resources.Context.DeviceContext.PSSetSampler(0, TextureSamplerState);

            Resources.Context.DeviceContext.VSSetConstantBuffer(0, Resources.SceneBuffer);

            Resources.Context.DeviceContext.PSSetConstantBuffer(0, Resources.MaterialBuffer);
            Resources.Context.DeviceContext.PSSetConstantBuffer(1, Resources.EnvironmentBuffer);
            Resources.Context.DeviceContext.PSSetConstantBuffer(2, Resources.SceneBuffer);

            Matrix4x4 viewProjection = context.ViewContext.View * context.ViewContext.Projection;

            SceneConstants sceneConstants = new()
            {
                ViewProjection = Matrix4x4.Transpose(viewProjection),
                CameraPosition = context.ViewContext.WorldPose.Pose.Position,
                LightColor = world.DirectionalLight.Color.ToLinear(),
                LightDirection = world.DirectionalLight.Direction,
                LightIntensity = world.DirectionalLight.Intensity * 0.001f,
                OutputMode = (uint)context.OutputMode,
                ViewportSizeInverse = new Vector2(1f / context.ViewportSize.Width, 1f / context.ViewportSize.Height),
            };
            Resources.Context.DeviceContext.UpdateSubresource(sceneConstants, Resources.SceneBuffer);

            EnvironmentConstants environmentConstants = new()
            {
                IBLIntensity = world.DefaultEnvironment.IBL.Intensity,
                IBLSaturation = world.DefaultEnvironment.IBL.Saturation,
            };
            Resources.Context.DeviceContext.UpdateSubresource(environmentConstants, Resources.EnvironmentBuffer);

            Resources.Context.ApplyState(IsReflect ? ReflectPipelineState : PipelineState);

            RenderQueue.SubmitBackground(Resources.Context.DeviceContext, context.ViewContext, world.BackgroundModels);
            Flush();
            Resources.Context.DeviceContext.ClearDepthStencilView(context.Surface.DepthStencil, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);

            Matrix4x4 cullingViewProjection = viewProjection;
            if (IsReflect) cullingViewProjection *= Matrix4x4.CreateScale(-1, 1, 1);
            FrustumCullingVolume culler = new(new BoundingFrustum(cullingViewProjection));

            RenderQueue.SubmitChunks(Resources.Context.DeviceContext, context.ViewContext, culler, world.Chunks, RenderLayer.Normal, context.Options.DrawChunkCount);
            RenderQueue.SubmitBodies(Resources.Context.DeviceContext, context.ViewContext, culler, world.Bodies, RenderLayer.Normal);
            Flush();


            void Flush()
            {
                RenderQueue.Render(new DrawContext()
                {
                    DeviceContext = Resources.Context.DeviceContext,
                    InstanceBuffer = Resources.InstanceBuffer,
                    InstanceCount = 0,
                    MaterialBuffer = Resources.MaterialBuffer,
                });
                RenderQueue.Clear();
            }
        }
    }
}
