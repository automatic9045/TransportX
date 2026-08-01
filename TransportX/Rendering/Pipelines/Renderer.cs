using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

using TransportX.Cameras;
using TransportX.Rendering.Backend;
using TransportX.Spatial;
using TransportX.Worlds;

namespace TransportX.Rendering.Pipelines
{
    public class Renderer : IRenderer
    {
        protected readonly Platform Platform;
        protected readonly IGraphicsHost GraphicsHost;
        protected readonly IGraphicsClient GraphicsClient;
        protected readonly RendererOptions Options;

        protected readonly RenderContext RenderContext;

        protected readonly ID3D11Buffer InstanceBuffer;
        protected readonly ID3D11Buffer MaterialBuffer;
        protected readonly ID3D11Buffer EnvironmentBuffer;
        protected readonly ID3D11Buffer SceneBuffer;

        protected readonly OpaquePass Opaque;
        protected readonly ShadowPass Shadow;
        protected readonly IBLPass IBL;
        protected readonly PostProcessingPass PostProcess;
        protected readonly DebugPass Debug;

        public Renderer(Platform platform, IGraphicsHost graphicsHost, IGraphicsClient graphicsClient, RendererOptions options)
        {
            Platform = platform;
            GraphicsHost = graphicsHost;
            GraphicsClient = graphicsClient;
            Options = options;

            RenderContext = new RenderContext(GraphicsHost.Context);


            InputElementDescription[] inputElements = [
                new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElementDescription("COLOR", 0, Format.R32G32B32A32_Float, InputElementDescription.AppendAligned, 0, InputClassification.PerVertexData, 0),
                new InputElementDescription("NORMAL", 0, Format.R32G32B32_Float, InputElementDescription.AppendAligned, 0, InputClassification.PerVertexData, 0),
                new InputElementDescription("TANGENT", 0, Format.R32G32B32_Float, InputElementDescription.AppendAligned, 0, InputClassification.PerVertexData, 0),
                new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, InputElementDescription.AppendAligned, 0, InputClassification.PerVertexData, 0),

                new InputElementDescription("WORLD", 0, Format.R32G32B32A32_Float, 0, 1, InputClassification.PerInstanceData, 1),
                new InputElementDescription("WORLD", 1, Format.R32G32B32A32_Float, InputElementDescription.AppendAligned, 1, InputClassification.PerInstanceData, 1),
                new InputElementDescription("WORLD", 2, Format.R32G32B32A32_Float, InputElementDescription.AppendAligned, 1, InputClassification.PerInstanceData, 1),
                new InputElementDescription("WORLD", 3, Format.R32G32B32A32_Float, InputElementDescription.AppendAligned, 1, InputClassification.PerInstanceData, 1),
            ];


            BufferDescription instanceBufferDesc = new()
            {
                Usage = ResourceUsage.Dynamic,
                ByteWidth = (uint)InstanceData.Size * 65536,
                BindFlags = BindFlags.VertexBuffer,
                CPUAccessFlags = CpuAccessFlags.Write,
                MiscFlags = 0,
            };
            InstanceBuffer = RenderContext.DeviceContext.Device.CreateBuffer(instanceBufferDesc);

            BufferDescription materialBufferDesc = new()
            {
                Usage = ResourceUsage.Default,
                ByteWidth = (uint)MaterialConstants.Size,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = 0,
            };
            MaterialBuffer = RenderContext.DeviceContext.Device.CreateBuffer(materialBufferDesc);

            BufferDescription environmentBufferDesc = new()
            {
                Usage = ResourceUsage.Default,
                ByteWidth = (uint)EnvironmentConstants.Size,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = 0,
            };
            EnvironmentBuffer = RenderContext.DeviceContext.Device.CreateBuffer(environmentBufferDesc);

            BufferDescription sceneBufferDesc = new()
            {
                Usage = ResourceUsage.Default,
                ByteWidth = (uint)SceneConstants.Size,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = 0,
            };
            SceneBuffer = RenderContext.DeviceContext.Device.CreateBuffer(sceneBufferDesc);


            Opaque = new OpaquePass(RenderContext, inputElements)
            {
                InstanceBuffer = InstanceBuffer,
                MaterialBuffer = MaterialBuffer,
                EnvironmentBuffer = EnvironmentBuffer,
                SceneBuffer = SceneBuffer,
            };

            Shadow = new ShadowPass(RenderContext, inputElements, Options.ShadowOptions)
            {
                InstanceBuffer = InstanceBuffer,
                MaterialBuffer = MaterialBuffer,
            };

            IBL = new IBLPass(RenderContext, inputElements)
            {
                InstanceBuffer = InstanceBuffer,
                MaterialBuffer = MaterialBuffer,
                EnvironmentBuffer = EnvironmentBuffer,
                SceneBuffer = SceneBuffer,
            };

            PostProcess = new PostProcessingPass(RenderContext);

            Debug = new DebugPass(RenderContext, inputElements)
            {
                InstanceBuffer = InstanceBuffer,
                MaterialBuffer = MaterialBuffer,
                SceneBuffer = SceneBuffer,
            };
        }

        public void Dispose()
        {
            Opaque.Dispose();
            Shadow.Dispose();
            IBL.Dispose();
            PostProcess.Dispose();
            Debug.Dispose();

            InstanceBuffer.Dispose();
            MaterialBuffer.Dispose();
            EnvironmentBuffer.Dispose();
            SceneBuffer.Dispose();
        }

        public void Render(ICamera camera, WorldBase world, TimeSpan elapsed)
        {
            if (GraphicsClient.DepthStencil is null) throw new InvalidOperationException();
            if (GraphicsClient.RenderTarget is null) throw new InvalidOperationException();

            SizeI size = new(Platform.Window.Size.X, Platform.Window.Size.Y);
            if (size.Width == 0 || size.Height == 0)
            {
                PostProcess.Reset();
                return;
            }

            ViewContext viewContext = camera.CreateViewContext(size);

            Shadow.UpdateCamera(world.DirectionalLight.Direction, viewContext);
            Shadow.Render(world.Chunks, world.Bodies);

            PostProcess.Setup(GraphicsClient.DepthStencil, size);

            GraphicsHost.Context.RSSetViewport(0, 0, size.Width, size.Height);
            GraphicsHost.Context.ClearDepthStencilView(GraphicsClient.DepthStencil, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);

            if (!IBL.IsGenerated)
            {
                IBL.Generate(world, camera.WorldPose);
            }

            Shadow.Bind();
            IBL.Bind();

            Opaque.Render(GraphicsClient.DepthStencil, world, viewContext, Options.DrawChunkCount, size);
            PostProcess.RenderTo(GraphicsClient.RenderTarget, world.DefaultEnvironment, elapsed);
            Debug.RenderTo(GraphicsClient.RenderTarget, GraphicsClient.DepthStencil, world, camera.VisibleLayers, viewContext, Options.DrawChunkCount, size);

            GraphicsHost.Context.PSSetShaderResource(12, null!);
        }
    }
}
