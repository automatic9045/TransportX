using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Input;
using Silk.NET.Maths;
using Vortice.DXGI;
using Vortice.Mathematics;

using TransportX.Cameras;
using TransportX.Data;
using TransportX.Input;
using TransportX.Physics;
using TransportX.Rendering.Backend;
using TransportX.Rendering.Pipelines;
using TransportX.Spatial;

namespace TransportX.Worlds
{
    public class WorldApp : IApp
    {
        protected readonly IAppHost Host;

        protected readonly DXHost DXHost;
        protected readonly DXClient DXClient;
        protected readonly PhysicsHost PhysicsHost;

        protected readonly Renderer Renderer;

        protected readonly TimeManager UpdateTimeManager;
        protected readonly TimeManager RenderTimeManager;
        protected readonly InputManager InputManager;
        protected readonly Camera Camera;

        protected readonly WorldBase World;

        protected readonly KeyObserver ReloadKeyObserver;
        protected readonly ViewpointInput ViewpointInput;
        protected readonly DebugInput DebugInput;

        private TimeSpan ComputingAccumulator = TimeSpan.Zero;
        protected TimeSpan LimitComputingTime { get; set; } = TimeSpan.FromSeconds(1d / 60);

        public bool IsDisposed { get; private set; } = false;

        public WorldApp(WorldAppDependencies dependencies)
        {
            Host = dependencies.Host;

            DXHost = dependencies.DXHost;
            DXClient = dependencies.DXClient;
            PhysicsHost = dependencies.PhysicsHost;

            Renderer = dependencies.Renderer;

            UpdateTimeManager = dependencies.UpdateTimeManager;
            RenderTimeManager = dependencies.RenderTimeManager;
            InputManager = dependencies.InputManager;
            Camera = dependencies.Camera;

            World = dependencies.World;

            ReloadKeyObserver = InputManager.ObserveKey(Key.F5);
            ReloadKeyObserver.Pressed += keyboard =>
            {
                DXHost.Context.ClearRenderTargetView(DXClient.RenderTarget, new Color4(0, 0, 0));
                DXClient.SwapChain!.Present(1, PresentFlags.None);

                Host.RequestLoadApp(Host.CurrentReference, new WorldAppParameters(World.Info));
            };

            ViewpointInput = new ViewpointInput(InputManager, Camera.Viewpoints);
            DebugInput = new DebugInput(InputManager, Camera);

            Host.Platform.Window.Update += OnUpdate;
            Host.Platform.Window.Render += OnRender;
            Host.Platform.Window.Resize += OnResize;

            Save save = Save.Import();
            if (save.FreeViewpointPose.HasValue)
            {
                Camera.Viewpoints.Free.Locate(save.FreeViewpointPose.Value);
            }

            World.OnStart();
        }

        public virtual void Dispose()
        {
            Host.Platform.Window.Update -= OnUpdate;
            Host.Platform.Window.Render -= OnRender;
            Host.Platform.Window.Resize -= OnResize;

            ReloadKeyObserver.Dispose();
            ViewpointInput.Dispose();
            DebugInput.Dispose();

            World.Dispose();

            PhysicsHost.Dispose();
            Renderer.Dispose();

            DXHost.Context.ClearState();
            DXHost.Context.Flush();

            DXClient.Dispose();
            DXHost.Dispose();

            Save save = new();
            if (Camera.Viewpoints.Current is FreeViewpoint viewpoint)
            {
                WorldPose worldPose = viewpoint.WorldPose;
                save.FreeViewpointPose = new CameraPose(worldPose.Chunk, worldPose.Pose.Position, viewpoint.Angle);
            }
            save.Export();

            IsDisposed = true;
        }

        private void OnUpdate(double deltaTime)
        {
            if (IsDisposed) return;

            ViewpointInput.ClientSize = new SizeI(Host.Platform.Window.Size.X, Host.Platform.Window.Size.Y);

            UpdateTimeManager.Tick(TimeSpan.FromSeconds(deltaTime));
            TimeSpan elapsed = UpdateTimeManager.DeltaTime;

            OnTick(elapsed);

            ComputingAccumulator += elapsed;
            while (LimitComputingTime <= ComputingAccumulator)
            {
                OnSubTick(LimitComputingTime);
                ComputingAccumulator -= LimitComputingTime;
            }
        }

        private void OnRender(double deltaTime)
        {
            RenderTimeManager.Tick(TimeSpan.FromSeconds(deltaTime));

            OnRender(RenderTimeManager.DeltaTime);
            DXClient.SwapChain!.Present(1, PresentFlags.None);
        }

        private void OnResize(Vector2D<int> size)
        {
            if (0 < size.X && 0 < size.Y)
            {
                DXClient.Resize(DXHost.Device, size.X, size.Y);
            }
        }

        protected virtual void OnSubTick(TimeSpan elapsed)
        {
            PhysicsHost.Simulation.Timestep((float)elapsed.TotalSeconds, PhysicsHost.ThreadDispatcher);
            World.SubTick(elapsed);
        }

        protected virtual void OnTick(TimeSpan elapsed)
        {
            string chunkText = $"({Camera.WorldPose.Chunk.X}, {Camera.WorldPose.Chunk.Z})";
            string coordText = $"({Camera.WorldPose.WorldPosition.X:F1}, {Camera.WorldPose.WorldPosition.Y:F1}, {Camera.WorldPose.WorldPosition.Z:F1})";
            Host.Platform.Window.Title = $"TransportX {chunkText}; {coordText} @ {RenderTimeManager.Frequency:f0} fps";

            World.Tick(elapsed);
        }

        protected virtual void OnRender(TimeSpan elapsed)
        {
            Renderer.Render(Camera, World, elapsed);
        }
    }
}
