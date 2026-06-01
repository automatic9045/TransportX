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
    public class DebugPass : IDisposable
    {
        private static readonly RenderLayer[] DebugLayers = Enum.GetValues<RenderLayer>().Where(layer => layer != RenderLayer.Normal).ToArray();


        protected readonly RenderContext RenderContext;

        protected readonly GraphicsPipelineState PipelineState;

        protected readonly RenderQueue RenderQueue = new();

        public required ID3D11Buffer InstanceBuffer { protected get; init; }
        public required ID3D11Buffer MaterialBuffer { protected get; init; }
        public required ID3D11Buffer SceneBuffer { protected get; init; }

        public DebugPass(RenderContext renderContext, InputElementDescription[] elements)
        {
            RenderContext = renderContext;


            Blob vsBlob = ShaderFactory.CompileFromResource("VS.hlsl", "main", "VS", "vs_5_0");
            ID3D11VertexShader vertexShader = RenderContext.DeviceContext.Device.CreateVertexShader(vsBlob);

            Blob psBlob = ShaderFactory.CompileFromResource("DebugPS.hlsl", "main", "DebugPS", "ps_5_0");
            ID3D11PixelShader pixelShader = RenderContext.DeviceContext.Device.CreatePixelShader(psBlob);

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

        public void RenderTo(ID3D11RenderTargetView renderTarget, ID3D11DepthStencilView depthStencil, Camera camera, WorldBase world, int drawChunkCount, SizeI size)
        {
            RenderContext.DeviceContext.OMSetRenderTargets(renderTarget, depthStencil);
            RenderContext.DeviceContext.RSSetViewport(0, 0, size.Width, size.Height);

            RenderContext.DeviceContext.ClearDepthStencilView(depthStencil, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);

            RenderContext.DeviceContext.VSSetConstantBuffer(0, SceneBuffer);
            RenderContext.DeviceContext.PSSetConstantBuffer(0, MaterialBuffer);

            RenderContext.ApplyState(PipelineState);

            foreach (RenderLayer layer in DebugLayers)
            {
                switch (layer)
                {
                    case RenderLayer.Colliders:
                        if (!camera.VisibleLayers.HasFlag(Camera.VisualLayers.Colliders)) continue;
                        break;

                    case RenderLayer.Network:
                        if (!camera.VisibleLayers.HasFlag(Camera.VisualLayers.Network)) continue;
                        break;

                    case RenderLayer.Traffic:
                        if (!camera.VisibleLayers.HasFlag(Camera.VisualLayers.Traffic)) continue;
                        break;
                }

                RenderQueue.SubmitChunks(RenderContext.DeviceContext, camera, world.Chunks, layer, drawChunkCount);
                RenderQueue.SubmitBodies(RenderContext.DeviceContext, camera, world.Bodies, layer);

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
    }
}
