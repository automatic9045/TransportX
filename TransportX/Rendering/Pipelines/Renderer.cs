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
using TransportX.Worlds;

namespace TransportX.Rendering.Pipelines
{
    public class Renderer : IDisposable
    {
        protected readonly Platform Platform;
        protected readonly IDXHost DXHost;
        protected readonly IDXClient DXClient;
        protected readonly RendererOptions Options;

        protected readonly RenderContext RenderContext;

        public readonly ID3D11Buffer InstanceBuffer;
        public readonly ID3D11Buffer MaterialBuffer;
        public readonly ID3D11Buffer EnvironmentBuffer;
        public readonly ID3D11Buffer SceneBuffer;

        protected readonly OpaquePass Opaque;
        protected readonly ShadowPass Shadow;
        protected readonly IBLPass IBL;
        protected readonly PostProcessingPass PostProcess;
        protected readonly DebugPass Debug;

        public Renderer(Platform platform, IDXHost dxHost, IDXClient dxClient, RendererOptions options)
        {
            Platform = platform;
            DXHost = dxHost;
            DXClient = dxClient;
            Options = options;

            RenderContext = new RenderContext(DXHost.Context);


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

        public void Render(Camera camera, WorldBase world, TimeSpan elapsed)
        {
            if (DXClient.DepthStencil is null) throw new InvalidOperationException();
            if (DXClient.RenderTarget is null) throw new InvalidOperationException();

            SizeI size = new(Platform.Window.Size.X, Platform.Window.Size.Y);
            if (size.Width == 0 || size.Height == 0)
            {
                PostProcess.Reset();
                return;
            }

            Shadow.UpdateCamera(world.DirectionalLight.Direction, world.Camera);
            Shadow.Render(world.Chunks, world.Bodies);

            PostProcess.Setup(DXClient.DepthStencil, size);

            DXHost.Context.RSSetViewport(0, 0, size.Width, size.Height);
            DXHost.Context.ClearDepthStencilView(DXClient.DepthStencil, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);

            if (!IBL.IsGenerated)
            {
                IBL.Generate(camera, world);
            }

            Shadow.Bind();
            IBL.Bind();

            Opaque.Render(DXClient.DepthStencil, camera, world, Options.DrawChunkCount, size);
            PostProcess.RenderTo(DXClient.RenderTarget, world.DefaultEnvironment, elapsed);
            Debug.RenderTo(DXClient.RenderTarget, camera, world, Options.DrawChunkCount, size);

            DXHost.Context.PSSetShaderResource(12, null!);
        }
    }
}
