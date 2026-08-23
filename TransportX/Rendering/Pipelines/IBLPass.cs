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

using TransportX.Rendering.Backend;
using TransportX.Spatial;
using TransportX.Worlds;

namespace TransportX.Rendering.Pipelines
{
    public class IBLPass : IRenderPass
    {
        protected readonly RenderResourceSet Resources;

        protected readonly GraphicsPipelineState PipelineState;
        protected readonly ID3D11SamplerState TextureSamplerState;
        protected readonly ID3D11SamplerState BrdfSamplerState;
        protected readonly RenderTextureArray CubeTexture;
        protected readonly ID3D11ShaderResourceView BrdfLutTexture;

        protected readonly RenderQueue RenderQueue = new();

        public bool IsGenerated { get; private set; } = false;

        public IBLPass(RenderResourceSet resources)
        {
            Resources = resources;
            ID3D11Device device = Resources.Context.DeviceContext.Device;


            Blob vsBlob = ShaderFactory.CompileFromResource("VS.hlsl", "main", "VS", "vs_5_0");
            ID3D11VertexShader vertexShader = device.CreateVertexShader(vsBlob);

            Blob psBlob = ShaderFactory.CompileFromResource("PS.hlsl", "main", "PS", "ps_5_0");
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
            TextureSamplerState = device.CreateSamplerState(textureSamplerDesc);

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
            BrdfSamplerState = device.CreateSamplerState(brdfSamplerDesc);

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
            CubeTexture = new RenderTextureArray(device, cubeTextureDesc, 6);

            using Stream brdfLutStream = ShaderFactory.GetShaderStream("Brdf.dds")!;
            byte[] brdfLutData = new byte[brdfLutStream.Length];
            brdfLutStream.ReadExactly(brdfLutData);
            BrdfLutTexture = new DDSTextureFactory(device).CreateFromMemory(brdfLutData);
        }

        public void Dispose()
        {
            PipelineState.Dispose();
            TextureSamplerState.Dispose();
            BrdfSamplerState.Dispose();
            CubeTexture.Dispose();
            BrdfLutTexture.Dispose();
        }

        public void Execute(in RenderPassContext context, WorldBase world)
        {
            if (!IsGenerated)
            {
                Generate(world, context.Camera.WorldPose, context.ViewportSize);
            }
            Bind();
        }

        private void Generate(WorldBase world, WorldPose cameraWorldPose, SizeI viewportSize)
        {
            ID3D11DeviceContext deviceContext = Resources.Context.DeviceContext;

            Viewport originalViewport = deviceContext.RSGetViewport();
            deviceContext.RSSetViewport(0, 0, 128, 128);
            deviceContext.PSSetSampler(1, BrdfSamplerState);

            Resources.Context.ApplyState(PipelineState);

            EnvironmentConstants envConstants = new()
            {
                IBLIntensity = world.DefaultEnvironment.IBL.Intensity,
                IBLSaturation = world.DefaultEnvironment.IBL.Saturation,
            };
            deviceContext.UpdateSubresource(envConstants, Resources.EnvironmentBuffer);

            deviceContext.VSSetConstantBuffer(0, Resources.SceneBuffer);
            deviceContext.PSSetConstantBuffer(0, Resources.MaterialBuffer);
            deviceContext.PSSetConstantBuffer(1, Resources.EnvironmentBuffer);
            deviceContext.PSSetConstantBuffer(2, Resources.SceneBuffer);
            deviceContext.PSSetSampler(0, TextureSamplerState);

            Vector3 cameraPosition = cameraWorldPose.Pose.Position;
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
                deviceContext.OMSetRenderTargets(CubeTexture.RenderTargetViews[i]);
                deviceContext.ClearRenderTargetView(CubeTexture.RenderTargetViews[i], Colors.Gray);

                Matrix4x4 view = Matrix4x4.CreateLookAtLeftHanded(cameraPosition, targets[i], ups[i]);

                SceneConstants sceneConstants = new()
                {
                    ViewProjection = Matrix4x4.Transpose(view * projection),
                    CameraPosition = cameraPosition,
                    LightColor = world.DirectionalLight.Color.ToLinear(),
                    LightDirection = world.DirectionalLight.Direction,
                    LightIntensity = world.DirectionalLight.Intensity * 0.001f,
                    OutputMode = (uint)RenderPassOutputMode.Forward,
                    ViewportSizeInverse = new Vector2(1f / viewportSize.Width, 1f / viewportSize.Height),
                };
                deviceContext.UpdateSubresource(sceneConstants, Resources.SceneBuffer);

                ViewContext viewContext = new()
                {
                    View = view,
                    Projection = projection,
                    WorldPose = cameraWorldPose,
                };

                TransformedDrawContext drawContext = new()
                {
                    DeviceContext = deviceContext,
                    RenderQueue = RenderQueue,
                    ChunkOffset = ChunkIndex.Zero,
                    ViewContext = viewContext,
                    Layer = RenderLayer.Normal
                };

                foreach (TransformedModel model in world.BackgroundModels)
                {
                    model.Pose = new Pose(cameraPosition);
                    model.Draw(drawContext);
                }

                RenderQueue.Render(new DrawContext()
                {
                    DeviceContext = deviceContext,
                    InstanceBuffer = Resources.InstanceBuffer,
                    InstanceCount = 0,
                    MaterialBuffer = Resources.MaterialBuffer,
                });

                RenderQueue.Clear();
            }

            deviceContext.GenerateMips(CubeTexture.ShaderResourceView);

            IsGenerated = true;
            deviceContext.RSSetViewport(originalViewport);
        }

        private void Bind()
        {
            ID3D11DeviceContext deviceContext = Resources.Context.DeviceContext;

            deviceContext.PSSetSampler(1, BrdfSamplerState);

            deviceContext.PSSetShaderResource(10, CubeTexture.ShaderResourceView);
            deviceContext.PSSetShaderResource(11, BrdfLutTexture);
        }
    }
}
