using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;
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

        protected readonly RenderResourceSet Resources;
        protected readonly IReadOnlyList<IRenderPass> Passes;

        public Renderer(Platform platform, IGraphicsHost graphicsHost, IGraphicsClient graphicsClient, RendererOptions options)
        {
            Platform = platform;
            GraphicsHost = graphicsHost;
            GraphicsClient = graphicsClient;
            Options = options;

            RenderContext renderContext = new(GraphicsHost.Context);

            BufferDescription instanceBufferDesc = new()
            {
                Usage = ResourceUsage.Dynamic,
                ByteWidth = (uint)InstanceData.Size * 65536,
                BindFlags = BindFlags.VertexBuffer,
                CPUAccessFlags = CpuAccessFlags.Write,
                MiscFlags = 0,
            };
            ID3D11Buffer instanceBuffer = renderContext.DeviceContext.Device.CreateBuffer(instanceBufferDesc);

            BufferDescription materialBufferDesc = new()
            {
                Usage = ResourceUsage.Default,
                ByteWidth = (uint)MaterialConstants.Size,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = 0,
            };
            ID3D11Buffer materialBuffer = renderContext.DeviceContext.Device.CreateBuffer(materialBufferDesc);

            BufferDescription environmentBufferDesc = new()
            {
                Usage = ResourceUsage.Default,
                ByteWidth = (uint)EnvironmentConstants.Size,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = 0,
            };
            ID3D11Buffer environmentBuffer = renderContext.DeviceContext.Device.CreateBuffer(environmentBufferDesc);

            BufferDescription sceneBufferDesc = new()
            {
                Usage = ResourceUsage.Default,
                ByteWidth = (uint)SceneConstants.Size,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = 0,
            };
            ID3D11Buffer sceneBuffer = renderContext.DeviceContext.Device.CreateBuffer(sceneBufferDesc);


            Resources = new RenderResourceSet()
            {
                Context = renderContext,

                InstanceBuffer = instanceBuffer,
                MaterialBuffer = materialBuffer,
                EnvironmentBuffer = environmentBuffer,
                SceneBuffer = sceneBuffer,
            };

            OpaquePass opaquePass = new(Resources);
            ShadowPass shadowPass = new(Resources, Options.ShadowOptions);
            IBLPass iblPass = new(Resources);
            PostProcessPass postProcessPass = new(Resources);
            DebugPass debugPass = new(Resources);
            PostProcessSetupPass postProessSetupPass = new(Resources, postProcessPass);

            Passes = [shadowPass, postProessSetupPass, iblPass, opaquePass, postProcessPass, debugPass];
        }

        public void Dispose()
        {
            foreach (IRenderPass pass in Passes)
            {
                pass.Dispose();
            }

            Resources.Dispose();
        }

        public void Render(ICamera camera, WorldBase world, TimeSpan elapsed)
        {
            if (GraphicsClient.Surface is null) throw new InvalidOperationException();

            SizeI size = new(Platform.Window.Size.X, Platform.Window.Size.Y);
            ViewContext viewContext = camera.CreateViewContext(size);

            RenderPassContext context = new()
            {
                Surface = GraphicsClient.Surface.Value,
                Options = Options,
                Camera = camera,
                ViewContext = viewContext,
                OutputMode = RenderPassOutputMode.Deferred,
                ViewportSize = size,
                Elapsed = elapsed,
            };

            for (int i = 0; i < Passes.Count; i++)
            {
                Passes[i].Execute(context, world);
            }

            GraphicsHost.Context.PSSetShaderResource(12, null!);
        }
    }
}
