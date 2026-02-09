using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

using TransportX.Worlds;

namespace TransportX.Rendering
{
    public class Renderer : IDisposable
    {
        protected static readonly Vector3 Light = Vector3.Normalize(new Vector3(-1, -6, 2));


        protected readonly IDXHost DXHost;
        protected readonly IDXClient DXClient;

        protected readonly ID3D11VertexShader VertexShader;
        protected readonly ID3D11PixelShader PixelShader;
        protected readonly ID3D11InputLayout InputLayout;
        protected readonly ID3D11Buffer VertexConstantBuffer;
        protected readonly ID3D11Buffer PixelConstantBuffer;
        protected readonly ID3D11SamplerState TextureSamplerState;
        protected readonly ID3D11RasterizerState RasterizerState;
        protected readonly ID3D11BlendState BlendState;

        public Renderer(IDXHost dxHost, IDXClient dxClient)
        {
            DXHost = dxHost;
            DXClient = dxClient;

            Blob vsBlob = ShaderFactory.CompileFromResource(DXHost.Device, "VS.hlsl", "main", "VS", "vs_5_0");
            VertexShader = DXHost.Device.CreateVertexShader(vsBlob);

            InputElementDescription[] elements = [
                new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElementDescription("NORMAL", 0, Format.R32G32B32_Float, InputElementDescription.AppendAligned, 0, InputClassification.PerVertexData, 0),
                new InputElementDescription("COLOR", 0, Format.R32G32B32A32_Float, InputElementDescription.AppendAligned, 0, InputClassification.PerVertexData, 0),
                new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, InputElementDescription.AppendAligned, 0, InputClassification.PerVertexData, 0),
            ];

            InputLayout = DXHost.Device.CreateInputLayout(elements, vsBlob.AsSpan());

            Blob psBlob = ShaderFactory.CompileFromResource(DXHost.Device, "PS.hlsl", "main", "PS", "ps_5_0");
            PixelShader = DXHost.Device.CreatePixelShader(psBlob);

            BufferDescription vertexBufferDesc = new BufferDescription()
            {
                Usage = ResourceUsage.Default,
                ByteWidth = (uint)Rendering.VertexConstantBuffer.Size,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = 0,
            };
            VertexConstantBuffer = DXHost.Device.CreateBuffer(vertexBufferDesc);

            BufferDescription pixelBufferDesc = new BufferDescription()
            {
                Usage = ResourceUsage.Default,
                ByteWidth = (uint)Rendering.PixelConstantBuffer.Size,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = 0,
            };
            PixelConstantBuffer = DXHost.Device.CreateBuffer(pixelBufferDesc);

            SamplerDescription samplerDesc = new SamplerDescription()
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

            BlendDescription blendDesc = new BlendDescription()
            {
                AlphaToCoverageEnable = false,
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
        }

        public void Dispose()
        {
            VertexShader.Dispose();
            PixelShader.Dispose();
            InputLayout.Dispose();
            VertexConstantBuffer.Dispose();
            PixelConstantBuffer.Dispose();
            TextureSamplerState.Dispose();
            RasterizerState.Dispose();
            BlendState.Dispose();
        }

        public void Draw(Camera camera, WorldBase world, System.Drawing.Size size)
        {
            DXHost.Context.RSSetState(RasterizerState);
            DXHost.Context.OMSetBlendState(BlendState);
            DXHost.Context.RSSetViewport(0, 0, size.Width, size.Height);

            DXHost.Context.ClearRenderTargetView(DXClient.RenderTarget, Colors.Gray);
            DXHost.Context.ClearDepthStencilView(DXClient.DepthStencil, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);

            DXHost.Context.IASetInputLayout(InputLayout);

            DXHost.Context.VSSetShader(VertexShader);
            DXHost.Context.VSSetConstantBuffer(0, VertexConstantBuffer);
            DXHost.Context.PSSetShader(PixelShader);
            DXHost.Context.PSSetConstantBuffer(0, PixelConstantBuffer);
            DXHost.Context.PSSetSampler(0, TextureSamplerState);

            CameraDrawContext cameraContext = new(DXHost.Context, VertexConstantBuffer, PixelConstantBuffer, size, Vector3.Zero);
            camera.DrawBackground(cameraContext, world.BackgroundModels);
            DXHost.Context.ClearDepthStencilView(DXClient.DepthStencil, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);

            cameraContext = new(DXHost.Context, VertexConstantBuffer, PixelConstantBuffer, size, Light);
            camera.DrawPlates(cameraContext, world.Plates);
            camera.DrawBodies(cameraContext, world.Bodies);
        }
    }
}
