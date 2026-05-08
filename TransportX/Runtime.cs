using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Dependency;
using TransportX.Input;
using TransportX.Physics;
using TransportX.Rendering;
using TransportX.Spatial;
using TransportX.Worlds;

namespace TransportX
{
    public class Runtime : IRuntime
    {
        protected readonly Platform Platform;
        protected readonly IDXHost DXHost;
        protected readonly IDXClient DXClient;
        protected readonly PhysicsHost PhysicsHost;

        protected readonly Renderer Renderer;

        protected readonly TimeManager TimeManager;
        protected readonly InputManager InputManager;
        protected readonly Camera Camera;

        protected readonly ViewpointInput ViewpointInput;
        protected readonly DebugInput DebugInput;

        protected readonly WorldBase World;

        protected TimeSpan LimitComputingTime { get; set; } = TimeSpan.FromSeconds(1d / 60);

        public PluginLoadContext Context { get; }

        public Runtime(RuntimeCreationInfo info)
        {
            Context = info.Context;

            Platform = info.Platform;
            DXHost = info.DXHost;
            DXClient = info.DXClient;
            PhysicsHost = info.PhysicsHost;

            Renderer = info.Renderer;

            TimeManager = info.TimeManager;
            InputManager = info.InputManager;
            Camera = info.Camera;

            World = info.World;

            ViewpointInput = new ViewpointInput(InputManager, Camera.Viewpoints);
            DebugInput = new DebugInput(InputManager, Camera);

            World.OnStart();
        }

        public virtual void Dispose()
        {
            ViewpointInput.Dispose();
            DebugInput.Dispose();

            World.Dispose();

            PhysicsHost.Dispose();
            Renderer.Dispose();

            DXHost.Context.ClearState();
            DXHost.Context.Flush();

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
        }

        public virtual void Draw()
        {
            ViewpointInput.ClientSize = (Vector2)Platform.Window.Size;

            TimeManager.Tick();
            TimeSpan elapsed = TimeManager.DeltaTime;

            OnTick(elapsed);

            int subTickCount = (int)double.Ceiling(elapsed / LimitComputingTime);
            TimeSpan computeElapsed = elapsed / subTickCount;
            for (int i = 0; i < subTickCount; i++)
            {
                OnSubTick(computeElapsed);
            }

            OnDraw();
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
            Platform.Window.Title = $"Bus {chunkText}; {coordText} @ {TimeManager.Fps:f0} fps";

            World.Tick(elapsed);
        }

        protected virtual void OnDraw()
        {
            Renderer.Draw(Camera, World);
        }
    }
}
