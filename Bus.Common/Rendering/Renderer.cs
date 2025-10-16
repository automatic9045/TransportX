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

using Bus.Common.Input;
using Bus.Common.Physics;
using Bus.Common.Vehicles;
using Bus.Common.Worlds;

namespace Bus.Common.Rendering
{
    public class Renderer : IRenderer
    {
        protected readonly IDXHost DXHost;
        protected readonly PhysicsHost PhysicsHost;

        protected readonly ID3D11VertexShader VertexShader;
        protected readonly ID3D11PixelShader PixelShader;
        protected readonly ID3D11InputLayout InputLayout;
        protected readonly ID3D11Buffer ConstantBuffer;
        protected readonly ID3D11SamplerState TextureSamplerState;
        protected readonly ID3D11RasterizerState RasterizerState;
        protected readonly ID3D11BlendState BlendState;

        protected readonly TimeManager TimeManager;
        protected readonly InputManager InputManager;
        protected readonly Camera Camera;

        protected readonly ViewpointInput ViewpointInput;

        protected readonly WorldBase World;

        protected TimeSpan LimitComputingTime { get; set; } = TimeSpan.FromSeconds(1d / 60);

        public Renderer(IDXHost dxHost, IWorldInfo worldInfo)
        {
            DXHost = dxHost;
            PhysicsHost = PhysicsHost.Create();

            Blob vsBlob = ShaderFactory.CompileFromResource(DXHost.Device, "VS.hlsl", "main", "VS", "vs_5_0");
            VertexShader = DXHost.Device.CreateVertexShader(vsBlob);

            InputElementDescription[] elements = [
                new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, InputElementDescription.AppendAligned, 0, InputClassification.PerVertexData, 0),
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

            TimeManager = new TimeManager();
            InputManager = new InputManager();
            Camera = new Camera();

            ViewpointInput = new ViewpointInput(InputManager, Camera.Viewpoints);

            World = LoadWorld(worldInfo);

            VehicleBase vehicle = LoadVehicle(@"D:\★ソフト\バス\Bus\_out\samples\BasicSample\Bus.Sample.dll", "Sample");
            World.Bodies.Add(vehicle);
            Camera.Viewpoints.AttachedTo = vehicle;
        }

        protected virtual WorldBase LoadWorld(IWorldInfo worldInfo)
        {
            WorldBuilder worldBuilder = new WorldBuilder(worldInfo)
            {
                DXHost = DXHost,
                PhysicsHost = PhysicsHost,
                TimeManager = TimeManager,
                InputManager = InputManager,
                Camera = Camera,
            };

            WorldBase world = worldBuilder.Build();
            return world;
        }

        protected virtual VehicleBase LoadVehicle(string path, string? identifier)
        {
            VehicleBuilder vehicleBuilder = new VehicleBuilder()
            {
                DXHost = DXHost,
                PhysicsHost = PhysicsHost,
                TimeManager = TimeManager,
                InputManager = InputManager,
                Camera = Camera,
                World = World,
            };

            VehicleBase vehicle = vehicleBuilder.Build(path, identifier);
            return vehicle;
        }

        public virtual void Dispose()
        {
            World.Dispose();

            PhysicsHost.Dispose();

            VertexShader.Dispose();
            PixelShader.Dispose();
            InputLayout.Dispose();
            ConstantBuffer.Dispose();
            TextureSamplerState.Dispose();
            RasterizerState.Dispose();
            BlendState.Dispose();
        }

        public virtual void Draw(ID3D11RenderTargetView renderTarget, ID3D11DepthStencilView depthStencil, System.Drawing.Size size)
        {
            ViewpointInput.ClientSize = new Vector2(size.Width, size.Height);

            TimeManager.Tick();

            LocatableObject position = Camera;
            string plateText = $"({position.PlateX}, {position.PlateZ})";
            string coordText = $"({position.PositionInWorld.X:F1}, {position.PositionInWorld.Y:F1}, {position.PositionInWorld.Z:F1})";
            //Application.Current.MainWindow.Title = $"Bus {plateText}; {coordText} @ {TimeManager.Fps:f0} fps";

            TimeSpan elapsed = TimeManager.DeltaTime;
            int computeTickCount = (int)double.Ceiling(elapsed / LimitComputingTime);
            TimeSpan computeElapsed = elapsed / computeTickCount;
            for (int i = 0; i < computeTickCount; i++)
            {
                OnComputeTick(computeElapsed);
            }

            OnTick(elapsed);
            OnDraw(renderTarget, depthStencil, size);
        }

        protected virtual void OnComputeTick(TimeSpan elapsed)
        {
            World.ComputeTick(elapsed);

            PhysicsHost.Simulation.Timestep((float)elapsed.TotalSeconds);
        }

        protected virtual void OnTick(TimeSpan elapsed)
        {
            World.Tick(elapsed);
        }

        protected virtual void OnDraw(ID3D11RenderTargetView renderTarget, ID3D11DepthStencilView depthStencil, System.Drawing.Size size)
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

            Camera.DrawBackground(DXHost.Context, ConstantBuffer, World.BackgroundModels, size);
            DXHost.Context.ClearDepthStencilView(depthStencil, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
            Camera.DrawPlates(DXHost.Context, ConstantBuffer, World.Plates, size);
            Camera.DrawBodies(DXHost.Context, ConstantBuffer, World.Bodies, size);
        }

        public void OnKeyDown(System.Windows.Input.Key key) => InputManager.OnKeyDown(key);
        public void OnKeyUp(System.Windows.Input.Key key) => InputManager.OnKeyUp(key);
        public void OnMouseDragMove(System.Windows.Vector offset) => InputManager.OnMouseDragMove(offset);
        public void OnMouseWheel(int delta) => InputManager.OnMouseWheel(delta);
    }
}
