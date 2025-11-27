using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

using Bus.Common.Worlds;

namespace Bus.Common.Rendering
{
    public class Renderer : IDisposable
    {
        protected readonly IDXHost DXHost;

        protected readonly ID3D11VertexShader VertexShader;
        protected readonly ID3D11PixelShader PixelShader;
        protected readonly ID3D11InputLayout InputLayout;
        protected readonly ID3D11Buffer ConstantBuffer;
        protected readonly ID3D11SamplerState TextureSamplerState;
        protected readonly ID3D11RasterizerState RasterizerState;
        protected readonly ID3D11BlendState BlendState;

        public Renderer(IDXHost dxHost)
        {
            DXHost = dxHost;

            Blob vsBlob = ShaderFactory.CompileFromResource(DXHost.Device, "VS.hlsl", "main", "VS", "vs_5_0");
            VertexShader = DXHost.Device.CreateVertexShader(vsBlob);

            InputElementDescription[] elements = [
                new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, InputElementDescription.AppendAligned, 0, InputClassification.PerVertexData, 0),
                new InputElementDescription("COLOR", 0, Format.R32G32B32A32_Float, InputElementDescription.AppendAligned, 0, InputClassification.PerVertexData, 0),
            ];

            InputLayout = DXHost.Device.CreateInputLayout(elements, vsBlob.AsSpan());

            Blob psBlob = ShaderFactory.CompileFromResource(DXHost.Device, "PS.hlsl", "main", "PS", "ps_5_0");
            PixelShader = DXHost.Device.CreatePixelShader(psBlob);

            BufferDescription bufferDesc = new BufferDescription()
            {
                Usage = ResourceUsage.Default,
                ByteWidth = (uint)Rendering.ConstantBuffer.Size,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = 0,
            };

            ConstantBuffer = DXHost.Device.CreateBuffer(bufferDesc);

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
            ConstantBuffer.Dispose();
            TextureSamplerState.Dispose();
            RasterizerState.Dispose();
            BlendState.Dispose();
        }

        public void Draw(ID3D11RenderTargetView renderTarget, ID3D11DepthStencilView depthStencil, Camera camera, WorldBase world, System.Drawing.Size size)
        {
            DXHost.Context.RSSetState(RasterizerState);
            DXHost.Context.OMSetBlendState(BlendState);
            DXHost.Context.RSSetViewport(0, 0, size.Width, size.Height);

            DXHost.Context.ClearRenderTargetView(renderTarget, Colors.Gray);
            DXHost.Context.ClearDepthStencilView(depthStencil, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);

            DXHost.Context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
            DXHost.Context.IASetInputLayout(InputLayout);

            DXHost.Context.VSSetShader(VertexShader);
            DXHost.Context.VSSetConstantBuffer(0, ConstantBuffer);
            DXHost.Context.PSSetShader(PixelShader);
            DXHost.Context.PSSetSampler(0, TextureSamplerState);

            camera.DrawBackground(DXHost.Context, ConstantBuffer, world.BackgroundModels, size);
            DXHost.Context.ClearDepthStencilView(depthStencil, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
            camera.DrawPlates(DXHost.Context, ConstantBuffer, world.Plates, size);
            camera.DrawBodies(DXHost.Context, ConstantBuffer, world.Bodies, size);
        }
    }
}
