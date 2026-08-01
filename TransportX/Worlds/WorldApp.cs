using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Input;
using Silk.NET.Maths;
using Vortice.DXGI;
using Vortice.Mathematics;

using TransportX.Audio;
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
        private static readonly TimeSpan TitleUpdatingTime = TimeSpan.FromSeconds(0.125);


        protected readonly IAppHost Host;

        protected readonly GraphicsHost GraphicsHost;
        protected readonly GraphicsClient GraphicsClient;
        protected readonly AudioClient AudioClient;
        protected readonly PhysicsHost PhysicsHost;

        protected readonly ViewpointSet Viewpoints;
        protected readonly IRenderer Renderer;

        protected readonly TimeManager UpdateTimeManager;
        protected readonly TimeManager RenderTimeManager;

        protected readonly WorldBase World;

        protected readonly KeyObserver ReloadKeyObserver;
        protected readonly ViewpointInput ViewpointInput;
        protected readonly DebugInput DebugInput;

        private TimeSpan TitleUpdatingAccumulator = TimeSpan.Zero;

        private TimeSpan ComputingAccumulator = TimeSpan.Zero;
        protected TimeSpan LimitComputingTime { get; set; } = TimeSpan.FromSeconds(1d / 60);

        public bool IsDisposed { get; private set; } = false;

        public WorldApp(WorldAppDependencies dependencies)
        {
            Host = dependencies.Host;

            GraphicsHost = dependencies.GraphicsHost;
            GraphicsClient = dependencies.GraphicsClient;
            AudioClient = dependencies.AudioClient;
            PhysicsHost = dependencies.PhysicsHost;

            Viewpoints = dependencies.Viewpoints;
            Renderer = dependencies.Renderer;

            UpdateTimeManager = dependencies.UpdateTimeManager;
            RenderTimeManager = dependencies.RenderTimeManager;

            World = dependencies.World;

            ReloadKeyObserver = World.InputManager.ObserveKey(Key.F5);
            ReloadKeyObserver.Pressed += keyboard =>
            {
                GraphicsHost.Context.ClearRenderTargetView(GraphicsClient.RenderTarget, new Color4(0, 0, 0));
                GraphicsClient.SwapChain!.Present(1, PresentFlags.None);

                Host.RequestLoadApp(Host.CurrentReference, new WorldAppParameters(World.Info));
            };

            ViewpointInput = new ViewpointInput(World.InputManager, Viewpoints);
            DebugInput = new DebugInput(World.InputManager, World.Camera);

            Host.Platform.Window.Update += OnUpdate;
            Host.Platform.Window.Render += OnRender;
            Host.Platform.Window.Resize += OnResize;

            Save save = Save.Import();
            if (save.FreeViewpointPose.HasValue)
            {
                Viewpoints.Free.Locate(save.FreeViewpointPose.Value);
            }
            else
            {
                CameraPose cameraPose = CameraPose.FromWorldPose(World.DefaultCameraPose);
                Viewpoints.Free.Locate(cameraPose);
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

            GraphicsHost.Context.ClearState();
            GraphicsHost.Context.Flush();

            GraphicsClient.Dispose();
            GraphicsHost.Dispose();

            Save save = new();
            if (Viewpoints.Current is FreeViewpoint viewpoint)
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
            GraphicsClient.SwapChain!.Present(1, PresentFlags.None);
        }

        private void OnResize(Vector2D<int> size)
        {
            if (0 < size.X && 0 < size.Y)
            {
                GraphicsClient.Resize(GraphicsHost.Device, size.X, size.Y);
            }
        }

        protected virtual void OnSubTick(TimeSpan elapsed)
        {
            PhysicsHost.Simulation.Timestep((float)elapsed.TotalSeconds, PhysicsHost.ThreadDispatcher);
            SyncCamera();
            World.SubTick(elapsed);
        }

        protected virtual void OnTick(TimeSpan elapsed)
        {
            TitleUpdatingAccumulator += elapsed;
            while (TitleUpdatingTime <= TitleUpdatingAccumulator)
            {
                string chunkText = $"({World.Camera.WorldPose.Chunk.X}, {World.Camera.WorldPose.Chunk.Z})";
                string coordText = $"({World.Camera.WorldPose.WorldPosition.X:F1}, {World.Camera.WorldPose.WorldPosition.Y:F1}, {World.Camera.WorldPose.WorldPosition.Z:F1})";
                Host.Platform.Window.Title = $"TransportX {chunkText}; {coordText} @ {RenderTimeManager.Frequency:f0} fps";

                TitleUpdatingAccumulator -= TitleUpdatingTime;
            }

            SyncCamera();
            World.Tick(elapsed);
        }

        private void SyncCamera()
        {
            Viewpoints.AttachedTo = World.Avatar;
            World.Camera.Perspective = Viewpoints.Current.Perspective;
            World.Camera.UpdateView(Viewpoints.Current.WorldPose);

            World.UpdateCameraChunk();
        }

        protected virtual void OnRender(TimeSpan elapsed)
        {
            AudioClient.Update(World.Camera.WorldPose, World.Camera.Velocity);
            Renderer.Render(World.Camera, World, elapsed);
        }
    }
}
