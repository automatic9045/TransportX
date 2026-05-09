using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Input;
using Silk.NET.Maths;
using Vortice.DXGI;
using Vortice.Mathematics;

using TransportX.Input;
using TransportX.Physics;
using TransportX.Rendering;
using TransportX.Spatial;
using TransportX.Worlds;

namespace TransportX
{
    public class Runtime : IRuntime
    {
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

        public RuntimeHost Host { get; }
        public bool IsDisposed { get; private set; } = false;

        public Runtime(RuntimeCreationInfo info)
        {
            Host = info.Host;

            DXHost = info.DXHost;
            DXClient = info.DXClient;
            PhysicsHost = info.PhysicsHost;

            Renderer = info.Renderer;

            UpdateTimeManager = info.UpdateTimeManager;
            RenderTimeManager = info.RenderTimeManager;
            InputManager = info.InputManager;
            Camera = info.Camera;

            World = info.World;

            ReloadKeyObserver = InputManager.ObserveKey(Key.F5);
            ReloadKeyObserver.Pressed += keyboard =>
            {
                DXHost.Context.ClearRenderTargetView(DXClient.RenderTarget, new Color4(0, 0, 0));
                DXClient.SwapChain!.Present(1, PresentFlags.None);

                Host.RequestReload();
            };

            ViewpointInput = new ViewpointInput(InputManager, Camera.Viewpoints);
            DebugInput = new DebugInput(InputManager, Camera);

            Host.Platform.Window.Update += OnUpdate;
            Host.Platform.Window.Render += OnRender;
            Host.Platform.Window.Resize += OnResize;

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

            try
            {
                Process process = Process.GetCurrentProcess();
                string savePath = Path.Combine(Path.GetDirectoryName(process.MainModule!.FileName)!, "Save.dat");

                StringBuilder saveContentBuilder = new();
                saveContentBuilder.AppendLine(process.Id.ToString(CultureInfo.InvariantCulture));

                if (Camera.Viewpoints.Current is FreeViewpoint viewpoint)
                {
                    WorldPose worldPose = viewpoint.WorldPose;
                    saveContentBuilder.AppendLine(FormattableString.Invariant($"{worldPose.ChunkX},{worldPose.ChunkZ}"));
                    saveContentBuilder.AppendLine(FormattableString.Invariant($"{worldPose.Pose.Position.X},{worldPose.Pose.Position.Y},{worldPose.Pose.Position.Z}"));
                    saveContentBuilder.AppendLine(FormattableString.Invariant($"{viewpoint.Angle.X},{viewpoint.Angle.Y}"));
                }

                File.WriteAllText(savePath, saveContentBuilder.ToString());
            }
            catch { }

            IsDisposed = true;
        }

        private void OnUpdate(double deltaTime)
        {
            if (IsDisposed) return;

            ViewpointInput.ClientSize = (Vector2)Host.Platform.Window.Size;

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

            OnDraw();
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
            string chunkText = $"({Camera.WorldPose.ChunkX}, {Camera.WorldPose.ChunkZ})";
            string coordText = $"({Camera.WorldPose.WorldPosition.X:F1}, {Camera.WorldPose.WorldPosition.Y:F1}, {Camera.WorldPose.WorldPosition.Z:F1})";
            Host.Platform.Window.Title = $"TransportX {chunkText}; {coordText} @ {RenderTimeManager.Frequency:f0} fps";

            World.Tick(elapsed);
        }

        protected virtual void OnDraw()
        {
            Renderer.Draw(Camera, World);
        }
    }
}
