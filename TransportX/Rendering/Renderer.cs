using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

using TransportX.Cameras;
using TransportX.Environment;
using TransportX.Rendering.Shadows;
using TransportX.Worlds;

namespace TransportX.Rendering
{
    public class Renderer : IDisposable
    {
        protected readonly Platform Platform;
        protected readonly IDXHost DXHost;
        protected readonly IDXClient DXClient;
        protected readonly RendererOptions Options;

        protected readonly ID3D11VertexShader VertexShader;
        protected readonly ID3D11PixelShader PixelShader;
        protected readonly ID3D11PixelShader DebugPixelShader;

        protected readonly ID3D11InputLayout InputLayout;

        protected readonly ID3D11Buffer InstanceBuffer;
        protected readonly ID3D11Buffer MaterialBuffer;
        protected readonly ID3D11Buffer EnvironmentBuffer;
        protected readonly ID3D11Buffer SceneBuffer;

        protected readonly ID3D11SamplerState TextureSamplerState;
        protected readonly ID3D11RasterizerState RasterizerState;
        protected readonly ID3D11BlendState BlendState;

        protected readonly ShadowPipeline Shadow;
        protected readonly IBLPipeline IBL;
        protected readonly PostProcessingPipeline PostProcess;

        public Renderer(Platform platform, IDXHost dxHost, IDXClient dxClient, RendererOptions options)
        {
            Platform = platform;
            DXHost = dxHost;
            DXClient = dxClient;
            Options = options;


            Blob vsBlob = ShaderFactory.CompileFromResource(DXHost.Device, "VS.hlsl", "main", "VS", "vs_5_0");
            VertexShader = DXHost.Device.CreateVertexShader(vsBlob);

            Blob psBlob = ShaderFactory.CompileFromResource(DXHost.Device, "PS.hlsl", "main", "PS", "ps_5_0");
            PixelShader = DXHost.Device.CreatePixelShader(psBlob);

            Blob debugPsBlob = ShaderFactory.CompileFromResource(DXHost.Device, "DebugPS.hlsl", "main", "DebugPS", "ps_5_0");
            DebugPixelShader = DXHost.Device.CreatePixelShader(debugPsBlob);


            InputElementDescription[] elements = [
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

            InputLayout = DXHost.Device.CreateInputLayout(elements, vsBlob.AsSpan());


            BufferDescription instanceBufferDesc = new()
            {
                Usage = ResourceUsage.Dynamic,
                ByteWidth = (uint)InstanceData.Size * 65536,
                BindFlags = BindFlags.VertexBuffer,
                CPUAccessFlags = CpuAccessFlags.Write,
                MiscFlags = 0,
            };
            InstanceBuffer = DXHost.Device.CreateBuffer(instanceBufferDesc);

            BufferDescription materialBufferDesc = new()
            {
                Usage = ResourceUsage.Default,
                ByteWidth = (uint)MaterialConstants.Size,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = 0,
            };
            MaterialBuffer = DXHost.Device.CreateBuffer(materialBufferDesc);

            BufferDescription environmentBufferDesc = new()
            {
                Usage = ResourceUsage.Default,
                ByteWidth = (uint)EnvironmentConstants.Size,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = 0,
            };
            EnvironmentBuffer = DXHost.Device.CreateBuffer(environmentBufferDesc);

            BufferDescription sceneBufferDesc = new()
            {
                Usage = ResourceUsage.Default,
                ByteWidth = (uint)SceneConstants.Size,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = 0,
            };
            SceneBuffer = DXHost.Device.CreateBuffer(sceneBufferDesc);


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
            TextureSamplerState = DXHost.Device.CreateSamplerState(samplerDesc);

            RasterizerDescription rasterizerDesc = new RasterizerDescription()
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
            RasterizerState = DXHost.Device.CreateRasterizerState(rasterizerDesc);

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
            BlendState = DXHost.Device.CreateBlendState(blendDesc);


            Shadow = new ShadowPipeline(DXHost, elements, Options.ShadowOptions)
            {
                InstanceBuffer = InstanceBuffer,
                MaterialBuffer = MaterialBuffer,
            };

            IBL = new IBLPipeline(DXHost)
            {
                PixelShader = PixelShader,
                InstanceBuffer = InstanceBuffer,
                MaterialBuffer = MaterialBuffer,
                SceneBuffer = SceneBuffer,
            };

            PostProcess = new PostProcessingPipeline(DXHost.Context);
        }

        public void Dispose()
        {
            VertexShader.Dispose();
            PixelShader.Dispose();
            DebugPixelShader.Dispose();

            InputLayout.Dispose();

            InstanceBuffer.Dispose();
            MaterialBuffer.Dispose();
            EnvironmentBuffer.Dispose();
            SceneBuffer.Dispose();

            TextureSamplerState.Dispose();
            RasterizerState.Dispose();
            BlendState.Dispose();

            Shadow.Dispose();
            PostProcess.Dispose();
        }

        public void Draw(Camera camera, WorldBase world)
        {
            if (DXClient.DepthStencil is null) throw new InvalidOperationException();
            if (DXClient.RenderTarget is null) throw new InvalidOperationException();

            SizeI size = new(Platform.Window.Size.X, Platform.Window.Size.Y);
            if (size.Width == 0 || size.Height == 0)
            {
                PostProcess.Reset();
                return;
            }

            DXHost.Context.RSSetState(RasterizerState);
            DXHost.Context.OMSetBlendState(BlendState);
            DXHost.Context.RSSetViewport(0, 0, size.Width, size.Height);

            DXHost.Context.VSSetConstantBuffer(0, SceneBuffer);

            DXHost.Context.PSSetConstantBuffer(0, MaterialBuffer);
            DXHost.Context.PSSetConstantBuffer(1, EnvironmentBuffer);
            DXHost.Context.PSSetConstantBuffer(2, SceneBuffer);

            DXHost.Context.PSSetSampler(0, TextureSamplerState);

            camera.UpdateProjection(size);

            SceneConstants sceneConstants = new()
            {
                ViewProjection = Matrix4x4.Transpose(camera.View * camera.Projection),
                CameraPosition = camera.WorldPose.Pose.Position,
                LightColor = world.DirectionalLight.Color.ToLinear(),
                LightDirection = world.DirectionalLight.Direction,
                LightIntensity = world.DirectionalLight.Intensity,
            };
            DXHost.Context.UpdateSubresource(sceneConstants, SceneBuffer);

            EnvironmentProfile environment = world.DefaultEnvironment;
            EnvironmentConstants environmentConstants = new()
            {
                IBLIntensity = environment.IBL.Intensity,
                IBLSaturation = environment.IBL.Saturation,
            };
            DXHost.Context.UpdateSubresource(environmentConstants, EnvironmentBuffer);

            Shadow.Render(world.DirectionalLight.Direction, world);
            PostProcess.Setup(DXClient.DepthStencil, size);

            DXHost.Context.RSSetViewport(0, 0, size.Width, size.Height);
            DXHost.Context.ClearDepthStencilView(DXClient.DepthStencil!, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);

            DXHost.Context.RSSetState(RasterizerState);

            DXHost.Context.VSSetShader(VertexShader);
            DXHost.Context.IASetInputLayout(InputLayout);

            if (!IBL.IsGenerated) IBL.Generate(camera, world);

            Shadow.Bind();
            IBL.Bind();

            CameraDrawContext cameraContext = new()
            {
                DeviceContext = DXHost.Context,
                PixelShader = PixelShader,
                DebugPixelShader = DebugPixelShader,
                InstanceBuffer = InstanceBuffer,
                MaterialBuffer = MaterialBuffer,
            };

            camera.DrawBackground(cameraContext, world.BackgroundModels);
            DXHost.Context.ClearDepthStencilView(DXClient.DepthStencil, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);

            camera.DrawChunks(cameraContext, world.Chunks);
            camera.DrawBodies(cameraContext, world.Bodies);

            PostProcess.RenderTo(DXClient.RenderTarget!, environment);

            DXHost.Context.PSSetShaderResource(12, null!);
        }
    }
}
