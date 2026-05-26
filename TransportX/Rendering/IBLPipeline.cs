using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

using TransportX.Cameras;
using TransportX.Environment;
using TransportX.Spatial;
using TransportX.Worlds;

namespace TransportX.Rendering
{
    public class IBLPipeline : IDisposable
    {
        protected readonly IDXHost DXHost;

        protected readonly RenderTextureArray CubeTexture;
        protected readonly RenderQueue RenderQueue;
        protected readonly ID3D11SamplerState BrdfSamplerState;
        protected readonly ID3D11ShaderResourceView BrdfLutTexture;

        public required ID3D11PixelShader PixelShader { protected get; init; }
        public required ID3D11Buffer InstanceBuffer { protected get; init; }
        public required ID3D11Buffer MaterialBuffer { protected get; init; }
        public required ID3D11Buffer SceneBuffer { protected get; init; }

        public bool IsGenerated { get; private set; } = false;

        public IBLPipeline(IDXHost dxHost)
        {
            DXHost = dxHost;

            Texture2DDescription textureDesc = new()
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
            CubeTexture = new RenderTextureArray(DXHost.Device, textureDesc, 6);

            RenderQueue = new RenderQueue();

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

            using Stream brdfLutStream = ShaderFactory.GetShaderStream("Brdf.dds")!;
            byte[] brdfLutData = new byte[brdfLutStream.Length];
            brdfLutStream.ReadExactly(brdfLutData);
            BrdfLutTexture = new DDSTextureFactory(DXHost.Device).CreateFromMemory(brdfLutData);
        }

        public void Dispose()
        {
            CubeTexture.Dispose();
            BrdfSamplerState.Dispose();
            BrdfLutTexture.Dispose();
        }

        public void Generate(Camera camera, WorldBase world)
        {
            Viewport originalViewport = DXHost.Context.RSGetViewport();
            DXHost.Context.RSSetViewport(0, 0, 128, 128);
            DXHost.Context.PSSetSampler(1, BrdfSamplerState);

            Vector3 cameraPosition = camera.WorldPose.Pose.Position;
            Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(float.Pi / 2, 1, 0.1f, 1000);

            Vector3[] targets = [
                cameraPosition + Vector3.UnitX,
                cameraPosition - Vector3.UnitX,
                cameraPosition + Vector3.UnitY,
                cameraPosition - Vector3.UnitY,
                cameraPosition + Vector3.UnitZ,
                cameraPosition - Vector3.UnitZ,
            ];

            Vector3[] ups = [
                Vector3.UnitY,
                Vector3.UnitY,
                -Vector3.UnitZ,
                Vector3.UnitZ,
                Vector3.UnitY,
                Vector3.UnitY,
            ];

            for (int i = 0; i < 6; i++)
            {
                DXHost.Context.OMSetRenderTargets(CubeTexture.RenderTargetViews[i]);
                DXHost.Context.ClearRenderTargetView(CubeTexture.RenderTargetViews[i], Colors.Gray);

                Matrix4x4 view = Matrix4x4.CreateLookAtLeftHanded(cameraPosition, targets[i], ups[i]);

                SceneConstants sceneConstants = new()
                {
                    ViewProjection = Matrix4x4.Transpose(view * projection),
                    CameraPosition = cameraPosition,
                    LightColor = world.DirectionalLight.Color.ToLinear(),
                    LightDirection = world.DirectionalLight.Direction,
                    LightIntensity = world.DirectionalLight.Intensity * 0.001f,
                };
                DXHost.Context.UpdateSubresource(sceneConstants, SceneBuffer);

                TransformedDrawContext drawContext = new()
                {
                    DeviceContext = DXHost.Context,
                    RenderQueue = RenderQueue,
                    ChunkOffset = ChunkOffset.Identity,
                    View = view,
                    Projection = projection,
                    Frustum = new BoundingFrustum(view * projection),
                    Pass = RenderPass.Normal
                };

                foreach (TransformedModel model in world.BackgroundModels)
                {
                    model.Pose = new Pose(cameraPosition);
                    model.Draw(drawContext);
                }

                DXHost.Context.PSSetShader(PixelShader);

                RenderQueue.Render(RenderPass.Normal, new DrawContext()
                {
                    DeviceContext = DXHost.Context,
                    InstanceBuffer = InstanceBuffer,
                    InstanceCount = 0,
                    MaterialBuffer = MaterialBuffer,
                });

                RenderQueue.Clear();
            }

            DXHost.Context.GenerateMips(CubeTexture.ShaderResourceView);

            IsGenerated = true;
            DXHost.Context.RSSetViewport(originalViewport);
        }

        public void Bind()
        {
            DXHost.Context.PSSetSampler(1, BrdfSamplerState);

            DXHost.Context.PSSetShaderResource(10, CubeTexture.ShaderResourceView);
            DXHost.Context.PSSetShaderResource(11, BrdfLutTexture);
        }
    }
}
