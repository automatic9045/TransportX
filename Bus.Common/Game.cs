using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

using Bus.Common.Input;
using Bus.Common.Physics;
using Bus.Common.Rendering;
using Bus.Common.Vehicles;
using Bus.Common.Worlds;

namespace Bus.Common
{
    public class Game : IGame
    {
        protected readonly IDXHost DXHost;
        protected readonly PhysicsHost PhysicsHost;
        protected readonly Renderer Renderer;

        protected readonly TimeManager TimeManager;
        protected readonly InputManager InputManager;
        protected readonly Camera Camera;

        protected readonly ViewpointInput ViewpointInput;

        protected readonly WorldBase World;

        protected TimeSpan LimitComputingTime { get; set; } = TimeSpan.FromSeconds(1d / 60);

        public Game(IDXHost dxHost, IWorldInfo worldInfo)
        {
            DXHost = dxHost;
            PhysicsHost = PhysicsHost.Create();
            Renderer = new Renderer(DXHost);

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
            Renderer.Dispose();
        }

        public virtual void Draw(ID3D11RenderTargetView renderTarget, ID3D11DepthStencilView depthStencil, System.Drawing.Size clientSize)
        {
            ViewpointInput.ClientSize = new Vector2(clientSize.Width, clientSize.Height);

            TimeManager.Tick();

            LocatableObject position = Camera;
            string plateText = $"({position.PlateX}, {position.PlateZ})";
            string coordText = $"({position.PositionInWorld.X:F1}, {position.PositionInWorld.Y:F1}, {position.PositionInWorld.Z:F1})";
            //Application.Current.MainWindow.Title = $"Bus {plateText}; {coordText} @ {TimeManager.Fps:f0} fps";

            TimeSpan elapsed = TimeManager.DeltaTime;
            int subTickCount = (int)double.Ceiling(elapsed / LimitComputingTime);
            TimeSpan computeElapsed = elapsed / subTickCount;
            for (int i = 0; i < subTickCount; i++)
            {
                OnSubTick(computeElapsed);
            }

            OnTick(elapsed);
            OnDraw(renderTarget, depthStencil, clientSize);
        }

        protected virtual void OnSubTick(TimeSpan elapsed)
        {
            World.SubTick(elapsed);

            PhysicsHost.Simulation.Timestep((float)elapsed.TotalSeconds, PhysicsHost.ThreadDispatcher);
        }

        protected virtual void OnTick(TimeSpan elapsed)
        {
            World.Tick(elapsed);
        }

        protected virtual void OnDraw(ID3D11RenderTargetView renderTarget, ID3D11DepthStencilView depthStencil, System.Drawing.Size clientSize)
        {
            Renderer.Draw(renderTarget, depthStencil, Camera, World, clientSize);
        }

        public void OnKeyDown(System.Windows.Input.Key key) => InputManager.OnKeyDown(key);
        public void OnKeyUp(System.Windows.Input.Key key) => InputManager.OnKeyUp(key);
        public void OnMouseDragMove(System.Windows.Vector offset) => InputManager.OnMouseDragMove(offset);
        public void OnMouseWheel(int delta) => InputManager.OnMouseWheel(delta);
    }
}
