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
using TransportX.Rendering.Backend;
using TransportX.Spatial;
using TransportX.Worlds;

namespace TransportX.Rendering.Pipelines
{
    public class IBLPass : IDisposable
    {
        protected readonly RenderContext RenderContext;

        protected readonly GraphicsPipelineState PipelineState;
        protected readonly ID3D11SamplerState TextureSamplerState;
        protected readonly ID3D11SamplerState BrdfSamplerState;
        protected readonly RenderTextureArray CubeTexture;
        protected readonly ID3D11ShaderResourceView BrdfLutTexture;

        protected readonly RenderQueue RenderQueue = new();

        public required ID3D11Buffer InstanceBuffer { protected get; init; }
        public required ID3D11Buffer MaterialBuffer { protected get; init; }
        public required ID3D11Buffer EnvironmentBuffer { protected get; init; }
        public required ID3D11Buffer SceneBuffer { protected get; init; }

        public bool IsGenerated { get; private set; } = false;

        public IBLPass(RenderContext renderContext, InputElementDescription[] inputElements)
        {
            RenderContext = renderContext;


            Blob vsBlob = ShaderFactory.CompileFromResource("VS.hlsl", "main", "VS", "vs_5_0");
            ID3D11VertexShader vertexShader = RenderContext.DeviceContext.Device.CreateVertexShader(vsBlob);

            Blob psBlob = ShaderFactory.CompileFromResource("PS.hlsl", "main", "PS", "ps_5_0");
            ID3D11PixelShader pixelShader = RenderContext.DeviceContext.Device.CreatePixelShader(psBlob);

            ID3D11InputLayout inputLayout = RenderContext.DeviceContext.Device.CreateInputLayout(inputElements, vsBlob.AsSpan());

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
                PrimitiveTopology = PrimitiveTopology.TriangleList
            };


            SamplerDescription textureSamplerDesc = new()
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
            TextureSamplerState = RenderContext.DeviceContext.Device.CreateSamplerState(textureSamplerDesc);

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
            BrdfSamplerState = RenderContext.DeviceContext.Device.CreateSamplerState(brdfSamplerDesc);

            Texture2DDescription cubeTextureDesc = new()
            {
                Width = 128,
                Height = 128,
                MipLevels = 0,
                ArraySize = 6,
                Format = Format.R16G16B16A16_Float,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.TextureCube | ResourceOptionFlags.GenerateMips,
            };
            CubeTexture = new RenderTextureArray(RenderContext.DeviceContext.Device, cubeTextureDesc, 6);

            using Stream brdfLutStream = ShaderFactory.GetShaderStream("Brdf.dds")!;
            byte[] brdfLutData = new byte[brdfLutStream.Length];
            brdfLutStream.ReadExactly(brdfLutData);
            BrdfLutTexture = new DDSTextureFactory(RenderContext.DeviceContext.Device).CreateFromMemory(brdfLutData);
        }

        public void Dispose()
        {
            PipelineState.Dispose();
            TextureSamplerState.Dispose();
            BrdfSamplerState.Dispose();
            CubeTexture.Dispose();
            BrdfLutTexture.Dispose();
        }

        public void Generate(Camera camera, WorldBase world)
        {
            Viewport originalViewport = RenderContext.DeviceContext.RSGetViewport();
            RenderContext.DeviceContext.RSSetViewport(0, 0, 128, 128);
            RenderContext.DeviceContext.PSSetSampler(1, BrdfSamplerState);

            RenderContext.ApplyState(PipelineState);

            EnvironmentConstants envConstants = new()
            {
                IBLIntensity = world.DefaultEnvironment.IBL.Intensity,
                IBLSaturation = world.DefaultEnvironment.IBL.Saturation,
            };
            RenderContext.DeviceContext.UpdateSubresource(envConstants, EnvironmentBuffer);

            RenderContext.DeviceContext.VSSetConstantBuffer(0, SceneBuffer);
            RenderContext.DeviceContext.PSSetConstantBuffer(0, MaterialBuffer);
            RenderContext.DeviceContext.PSSetConstantBuffer(1, EnvironmentBuffer);
            RenderContext.DeviceContext.PSSetConstantBuffer(2, SceneBuffer);
            RenderContext.DeviceContext.PSSetSampler(0, TextureSamplerState);

            Vector3 cameraPosition = camera.WorldPose.Pose.Position;
            Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(float.Pi / 2, 1, 0.1f, 1000);

            ReadOnlySpan<Vector3> targets = [
                cameraPosition + Vector3.UnitX,
                cameraPosition - Vector3.UnitX,
                cameraPosition + Vector3.UnitY,
                cameraPosition - Vector3.UnitY,
                cameraPosition + Vector3.UnitZ,
                cameraPosition - Vector3.UnitZ,
            ];

            ReadOnlySpan<Vector3> ups = [
                Vector3.UnitY,
                Vector3.UnitY,
                -Vector3.UnitZ,
                Vector3.UnitZ,
                Vector3.UnitY,
                Vector3.UnitY,
            ];

            for (int i = 0; i < 6; i++)
            {
                RenderContext.DeviceContext.OMSetRenderTargets(CubeTexture.RenderTargetViews[i]);
                RenderContext.DeviceContext.ClearRenderTargetView(CubeTexture.RenderTargetViews[i], Colors.Gray);

                Matrix4x4 view = Matrix4x4.CreateLookAtLeftHanded(cameraPosition, targets[i], ups[i]);

                SceneConstants sceneConstants = new()
                {
                    ViewProjection = Matrix4x4.Transpose(view * projection),
                    CameraPosition = cameraPosition,
                    LightColor = world.DirectionalLight.Color.ToLinear(),
                    LightDirection = world.DirectionalLight.Direction,
                    LightIntensity = world.DirectionalLight.Intensity * 0.001f,
                };
                RenderContext.DeviceContext.UpdateSubresource(sceneConstants, SceneBuffer);

                TransformedDrawContext drawContext = new()
                {
                    DeviceContext = RenderContext.DeviceContext,
                    RenderQueue = RenderQueue,
                    ChunkOffset = ChunkIndex.Zero,
                    View = view,
                    Projection = projection,
                    Frustum = new BoundingFrustum(view * projection),
                    Layer = RenderLayer.Normal
                };

                foreach (TransformedModel model in world.BackgroundModels)
                {
                    model.Pose = new Pose(cameraPosition);
                    model.Draw(drawContext);
                }

                RenderQueue.Render(RenderLayer.Normal, new DrawContext()
                {
                    DeviceContext = RenderContext.DeviceContext,
                    InstanceBuffer = InstanceBuffer,
                    InstanceCount = 0,
                    MaterialBuffer = MaterialBuffer,
                });

                RenderQueue.Clear();
            }

            RenderContext.DeviceContext.GenerateMips(CubeTexture.ShaderResourceView);

            IsGenerated = true;
            RenderContext.DeviceContext.RSSetViewport(originalViewport);
        }

        public void Bind()
        {
            RenderContext.DeviceContext.PSSetSampler(1, BrdfSamplerState);

            RenderContext.DeviceContext.PSSetShaderResource(10, CubeTexture.ShaderResourceView);
            RenderContext.DeviceContext.PSSetShaderResource(11, BrdfLutTexture);
        }
    }
}
