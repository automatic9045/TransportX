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

using TransportX.Environment;
using TransportX.Worlds;

namespace TransportX.Rendering
{
    public class Renderer : IDisposable
    {
        protected static readonly Vector3 LightDirection = Vector3.Normalize(new Vector3(-1, -6, -2));


        protected readonly IDXHost DXHost;
        protected readonly IDXClient DXClient;

        protected readonly ID3D11VertexShader VertexShader;
        protected readonly ID3D11InputLayout InputLayout;

        protected readonly ID3D11PixelShader PixelShader;
        protected readonly ID3D11PixelShader DebugPixelShader;

        protected readonly ID3D11Buffer TransformBuffer;
        protected readonly ID3D11Buffer MaterialBuffer;
        protected readonly ID3D11Buffer EnvironmentBuffer;
        protected readonly ID3D11Buffer SceneBuffer;

        protected readonly ID3D11SamplerState TextureSamplerState;
        protected readonly ID3D11SamplerState BrdfSamplerState;
        protected readonly ID3D11RasterizerState RasterizerState;
        protected readonly ID3D11BlendState BlendState;

        protected readonly PostProcessingPipeline PostProcess;

        private readonly ID3D11ShaderResourceView BrdfLutTexture;

        public Renderer(IDXHost dxHost, IDXClient dxClient)
        {
            DXHost = dxHost;
            DXClient = dxClient;

            Blob vsBlob = ShaderFactory.CompileFromResource(DXHost.Device, "VS.hlsl", "main", "VS", "vs_5_0");
            VertexShader = DXHost.Device.CreateVertexShader(vsBlob);

            InputElementDescription[] elements = [
                new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElementDescription("COLOR", 0, Format.R32G32B32A32_Float, InputElementDescription.AppendAligned, 0, InputClassification.PerVertexData, 0),
                new InputElementDescription("NORMAL", 0, Format.R32G32B32_Float, InputElementDescription.AppendAligned, 0, InputClassification.PerVertexData, 0),
                new InputElementDescription("TANGENT", 0, Format.R32G32B32_Float, InputElementDescription.AppendAligned, 0, InputClassification.PerVertexData, 0),
                new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, InputElementDescription.AppendAligned, 0, InputClassification.PerVertexData, 0),
            ];

            InputLayout = DXHost.Device.CreateInputLayout(elements, vsBlob.AsSpan());

            Blob psBlob = ShaderFactory.CompileFromResource(DXHost.Device, "PS.hlsl", "main", "PS", "ps_5_0");
            PixelShader = DXHost.Device.CreatePixelShader(psBlob);

            Blob debugPsBlob = ShaderFactory.CompileFromResource(DXHost.Device, "DebugPS.hlsl", "main", "DebugPS", "ps_5_0");
            DebugPixelShader = DXHost.Device.CreatePixelShader(debugPsBlob);

            BufferDescription transformBufferDesc = new()
            {
                Usage = ResourceUsage.Default,
                ByteWidth = (uint)Rendering.TransformBuffer.Size,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = 0,
            };
            TransformBuffer = DXHost.Device.CreateBuffer(transformBufferDesc);

            BufferDescription materialBufferDesc = new()
            {
                Usage = ResourceUsage.Default,
                ByteWidth = (uint)Rendering.MaterialBuffer.Size,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = 0,
            };
            MaterialBuffer = DXHost.Device.CreateBuffer(materialBufferDesc);

            BufferDescription environmentBufferDesc = new()
            {
                Usage = ResourceUsage.Default,
                ByteWidth = (uint)Rendering.EnvironmentBuffer.Size,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = 0,
            };
            EnvironmentBuffer = DXHost.Device.CreateBuffer(environmentBufferDesc);

            BufferDescription sceneBufferDesc = new()
            {
                Usage = ResourceUsage.Default,
                ByteWidth = (uint)Rendering.SceneBuffer.Size,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = 0,
            };
            SceneBuffer = DXHost.Device.CreateBuffer(sceneBufferDesc);

            SamplerDescription samplerDesc = new()
            {
                Filter = Filter.Anisotropic,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                ComparisonFunc = ComparisonFunction.Never,
                MinLOD = 0,
                MaxLOD = 2,
            };
            TextureSamplerState = DXHost.Device.CreateSamplerState(samplerDesc);

            SamplerDescription brdfSamplerDesc = new()
            {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                ComparisonFunc = ComparisonFunction.Never,
                MinLOD = 0,
                MaxLOD = float.MaxValue,
            };
            BrdfSamplerState = DXHost.Device.CreateSamplerState(brdfSamplerDesc);

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
                IndependentBlendEnable = false,
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
            BlendState = DXHost.Device.CreateBlendState(blendDesc);

            PostProcess = new PostProcessingPipeline(DXHost.Context);

            Stream brdfLutStream = ShaderFactory.GetShaderStream("Brdf.dds")!;
            byte[] brdfLutData = new byte[brdfLutStream.Length];
            brdfLutStream.ReadExactly(brdfLutData);
            BrdfLutTexture = new DDSTextureFactory(DXHost.Device).CreateFromMemory(brdfLutData);
        }

        public void Dispose()
        {
            VertexShader.Dispose();
            InputLayout.Dispose();

            PixelShader.Dispose();
            DebugPixelShader.Dispose();

            TransformBuffer.Dispose();
            MaterialBuffer.Dispose();
            EnvironmentBuffer.Dispose();
            SceneBuffer.Dispose();

            TextureSamplerState.Dispose();
            BrdfSamplerState.Dispose();
            RasterizerState.Dispose();
            BlendState.Dispose();

            PostProcess.Dispose();

            BrdfLutTexture.Dispose();
        }

        public void Draw(Camera camera, WorldBase world, System.Drawing.Size size)
        {
            if (DXClient.DepthStencil is null) throw new InvalidOperationException();
            if (DXClient.RenderTarget is null) throw new InvalidOperationException();

            PostProcess.Setup(DXClient.DepthStencil, size);

            DXHost.Context.RSSetState(RasterizerState);
            DXHost.Context.OMSetBlendState(BlendState);
            DXHost.Context.RSSetViewport(0, 0, size.Width, size.Height);
            DXHost.Context.IASetInputLayout(InputLayout);

            DXHost.Context.VSSetShader(VertexShader);
            DXHost.Context.VSSetConstantBuffer(0, TransformBuffer);

            DXHost.Context.PSSetConstantBuffer(0, MaterialBuffer);
            DXHost.Context.PSSetConstantBuffer(1, EnvironmentBuffer);
            DXHost.Context.PSSetConstantBuffer(2, SceneBuffer);

            DXHost.Context.PSSetSampler(0, TextureSamplerState);

            SceneBuffer sceneData = new()
            {
                CameraPosition = camera.Pose.Position,
                LightColor = world.DirectionalLight.Color.ToLinear(),
                LightDirection = world.DirectionalLight.Direction,
                LightIntensity = world.DirectionalLight.Intensity,
            };
            DXHost.Context.UpdateSubresource(sceneData, SceneBuffer);

            DXHost.Context.PSSetShaderResource(100, BrdfLutTexture);
            DXHost.Context.PSSetSampler(1, BrdfSamplerState);

            EnvironmentProfile environment = world.DefaultEnvironment;

            EnvironmentBuffer environmentData = new()
            {
                IBLIntensity = environment.IBL.Intensity,
                IBLSaturation = environment.IBL.Saturation,
            };
            DXHost.Context.UpdateSubresource(environmentData, EnvironmentBuffer);

            DXHost.Context.PSSetShaderResource(10, environment.IBL.DiffuseTexture!);
            DXHost.Context.PSSetShaderResource(11, environment.IBL.SpecularTexture!);


            CameraDrawContext cameraContext = new()
            {
                DeviceContext = DXHost.Context,
                PixelShader = PixelShader,
                DebugPixelShader = DebugPixelShader,
                TransformBuffer = TransformBuffer,
                MaterialBuffer = MaterialBuffer,
                ClientSize = size,
            };

            camera.DrawBackground(cameraContext, world.BackgroundModels);
            DXHost.Context.ClearDepthStencilView(DXClient.DepthStencil, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);

            camera.DrawPlates(cameraContext, world.Plates);
            camera.DrawBodies(cameraContext, world.Bodies);


            PostProcess.RenderTo(DXClient.RenderTarget, environment);
        }
    }
}
