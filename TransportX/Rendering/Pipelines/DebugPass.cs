using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.Mathematics;

using TransportX.Cameras;
using TransportX.Rendering.Backend;
using TransportX.Spatial;
using TransportX.Worlds;

namespace TransportX.Rendering.Pipelines
{
    public class DebugPass : IRenderPass
    {
        private static readonly RenderLayer[] DebugLayers = Enum.GetValues<RenderLayer>().Where(layer => layer != RenderLayer.Normal).ToArray();


        protected readonly RenderResourceSet Resources;

        protected readonly GraphicsPipelineState PipelineState;

        protected readonly RenderQueue RenderQueue = new();

        public DebugPass(RenderResourceSet resources)
        {
            Resources = resources;
            ID3D11Device device = Resources.Context.DeviceContext.Device;


            Blob vsBlob = ShaderFactory.CompileFromResource("VS.hlsl", "main", "VS", "vs_5_0");
            ID3D11VertexShader vertexShader = device.CreateVertexShader(vsBlob);

            Blob psBlob = ShaderFactory.CompileFromResource("DebugPS.hlsl", "main", "DebugPS", "ps_5_0");
            ID3D11PixelShader pixelShader = device.CreatePixelShader(psBlob);

            ID3D11InputLayout inputLayout = device.CreateInputLayout(IRenderer.DefaultInputElements.ToArray(), vsBlob);

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
            ID3D11RasterizerState rasterizerState = device.CreateRasterizerState(rasterizerDesc);

            PipelineState = new GraphicsPipelineState()
            {
                VertexShader = vertexShader,
                PixelShader = pixelShader,
                InputLayout = inputLayout,
                RasterizerState = rasterizerState,
                BlendState = null,
                DepthStencilState = null,
                PrimitiveTopology = PrimitiveTopology.TriangleList,
            };
        }

        public void Dispose()
        {
            PipelineState.Dispose();
        }

        public void Execute(in RenderPassContext context, WorldBase world)
        {
            ID3D11DeviceContext deviceContext = Resources.Context.DeviceContext;

            deviceContext.OMSetRenderTargets(context.Surface.RenderTarget!, context.Surface.DepthStencil!);
            deviceContext.RSSetViewport(0, 0, context.ViewportSize.Width, context.ViewportSize.Height);

            deviceContext.ClearDepthStencilView(context.Surface.DepthStencil!, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);

            deviceContext.VSSetConstantBuffer(0, Resources.SceneBuffer);
            deviceContext.PSSetConstantBuffer(0, Resources.MaterialBuffer);

            Resources.Context.ApplyState(PipelineState);

            FrustumCullingVolume culler = new(new BoundingFrustum(context.ViewContext.View * context.ViewContext.Projection));

            foreach (RenderLayer layer in DebugLayers)
            {
                switch (layer)
                {
                    case RenderLayer.Colliders:
                        if (!context.Camera.VisibleLayers.HasFlag(ICamera.VisualLayers.Colliders)) continue;
                        break;

                    case RenderLayer.Network:
                        if (!context.Camera.VisibleLayers.HasFlag(ICamera.VisualLayers.Network)) continue;
                        break;

                    case RenderLayer.Traffic:
                        if (!context.Camera.VisibleLayers.HasFlag(ICamera.VisualLayers.Traffic)) continue;
                        break;
                }

                RenderQueue.SubmitChunks(deviceContext, context.ViewContext, culler, world.Chunks, layer, context.Options.DrawChunkCount);
                RenderQueue.SubmitBodies(deviceContext, context.ViewContext, culler, world.Bodies, layer);

                RenderQueue.Render(new DrawContext()
                {
                    DeviceContext = deviceContext,
                    InstanceBuffer = Resources.InstanceBuffer,
                    InstanceCount = 0,
                    MaterialBuffer = Resources.MaterialBuffer,
                });
                RenderQueue.Clear();
            }
        }
    }
}
