using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.Mathematics;

using TransportX.Cameras;
using TransportX.Rendering.Backend;
using TransportX.Worlds;

namespace TransportX.Rendering.Pipelines
{
    public class OpaquePass : IDisposable
    {
        private static readonly RenderLayer[] AllLayers = Enum.GetValues<RenderLayer>();


        protected readonly RenderContext RenderContext;

        protected readonly GraphicsPipelineState PipelineState;
        protected readonly GraphicsPipelineState DebugPipelineState;
        protected readonly ID3D11SamplerState TextureSamplerState;

        protected readonly RenderQueue RenderQueue = new();

        public required ID3D11Buffer InstanceBuffer { protected get; init; }
        public required ID3D11Buffer MaterialBuffer { protected get; init; }
        public required ID3D11Buffer EnvironmentBuffer { protected get; init; }
        public required ID3D11Buffer SceneBuffer { protected get; init; }

        public OpaquePass(RenderContext renderContext, InputElementDescription[] elements)
        {
            RenderContext = renderContext;


            Blob vsBlob = ShaderFactory.CompileFromResource("VS.hlsl", "main", "VS", "vs_5_0");
            ID3D11VertexShader vertexShader = RenderContext.DeviceContext.Device.CreateVertexShader(vsBlob);

            Blob psBlob = ShaderFactory.CompileFromResource("PS.hlsl", "main", "PS", "ps_5_0");
            ID3D11PixelShader pixelShader = RenderContext.DeviceContext.Device.CreatePixelShader(psBlob);

            Blob debugPsBlob = ShaderFactory.CompileFromResource("DebugPS.hlsl", "main", "DebugPS", "ps_5_0");
            ID3D11PixelShader debugPixelShader = RenderContext.DeviceContext.Device.CreatePixelShader(debugPsBlob);

            ID3D11InputLayout inputLayout = RenderContext.DeviceContext.Device.CreateInputLayout(elements, vsBlob.AsSpan());

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
            ID3D11RasterizerState rasterizerState = RenderContext.DeviceContext.Device.CreateRasterizerState(rasterizerDesc);

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
            ID3D11BlendState blendState = RenderContext.DeviceContext.Device.CreateBlendState(blendDesc);

            PipelineState = new GraphicsPipelineState()
            {
                VertexShader = vertexShader,
                PixelShader = pixelShader,
                InputLayout = inputLayout,
                RasterizerState = rasterizerState,
                BlendState = blendState,
                DepthStencilState = null,
                PrimitiveTopology = PrimitiveTopology.TriangleList
            };

            DebugPipelineState = PipelineState with
            {
                PixelShader = debugPixelShader,
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
            TextureSamplerState = RenderContext.DeviceContext.Device.CreateSamplerState(samplerDesc);
        }

        public void Dispose()
        {
            PipelineState.Dispose();
            DebugPipelineState.PixelShader!.Dispose();

            TextureSamplerState.Dispose();
        }

        public void Render(ID3D11DepthStencilView depthStencil, Camera camera, WorldBase world, int drawChunkCount, SizeI size)
        {
            RenderContext.DeviceContext.RSSetViewport(0, 0, size.Width, size.Height);
            RenderContext.DeviceContext.PSSetSampler(0, TextureSamplerState);

            RenderContext.DeviceContext.VSSetConstantBuffer(0, SceneBuffer);

            RenderContext.DeviceContext.PSSetConstantBuffer(0, MaterialBuffer);
            RenderContext.DeviceContext.PSSetConstantBuffer(1, EnvironmentBuffer);
            RenderContext.DeviceContext.PSSetConstantBuffer(2, SceneBuffer);

            camera.UpdateProjection(size);

            SceneConstants sceneConstants = new()
            {
                ViewProjection = Matrix4x4.Transpose(camera.View * camera.Projection),
                CameraPosition = camera.WorldPose.Pose.Position,
                LightColor = world.DirectionalLight.Color.ToLinear(),
                LightDirection = world.DirectionalLight.Direction,
                LightIntensity = world.DirectionalLight.Intensity * 0.001f,
            };
            RenderContext.DeviceContext.UpdateSubresource(sceneConstants, SceneBuffer);

            EnvironmentConstants environmentConstants = new()
            {
                IBLIntensity = world.DefaultEnvironment.IBL.Intensity,
                IBLSaturation = world.DefaultEnvironment.IBL.Saturation,
            };
            RenderContext.DeviceContext.UpdateSubresource(environmentConstants, EnvironmentBuffer);

            RenderQueue.SubmitBackground(RenderContext.DeviceContext, camera, world.BackgroundModels);
            Flush(RenderQueue);
            RenderContext.DeviceContext.ClearDepthStencilView(depthStencil, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);

            RenderQueue.SubmitChunks(RenderContext.DeviceContext, camera, world.Chunks, drawChunkCount);
            RenderQueue.SubmitBodies(RenderContext.DeviceContext, camera, world.Bodies);
            Flush(RenderQueue);

        }

        protected void Flush(IRenderQueue renderQueue)
        {
            foreach (RenderLayer layer in AllLayers)
            {
                RenderContext.ApplyState(layer == RenderLayer.Normal ? PipelineState : DebugPipelineState);

                renderQueue.Render(layer, new DrawContext()
                {
                    DeviceContext = RenderContext.DeviceContext,
                    InstanceBuffer = InstanceBuffer,
                    InstanceCount = 0,
                    MaterialBuffer = MaterialBuffer,
                });
            }

            renderQueue.Clear();
        }
    }
}
