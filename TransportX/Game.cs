using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using TransportX.Dependency;
using TransportX.Diagnostics;
using TransportX.Input;
using TransportX.Physics;
using TransportX.Rendering;
using TransportX.Worlds;

namespace TransportX
{
    public class Game : IGame
    {
        protected readonly IDXHost DXHost;
        protected readonly IDXClient DXClient;
        protected readonly PhysicsHost PhysicsHost;

        protected readonly Renderer Renderer;

        protected readonly TimeManager TimeManager;
        protected readonly Input.InputManager InputManager;
        protected readonly Camera Camera;

        protected readonly ViewpointInput ViewpointInput;
        protected readonly DebugInput DebugInput;

        protected readonly WorldBase World;

        protected TimeSpan LimitComputingTime { get; set; } = TimeSpan.FromSeconds(1d / 60);

        public PluginLoadContext Context { get; }

        public Game(GameCreationInfo info)
        {
            Context = info.Context;

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
        }

        public virtual void Draw(System.Drawing.Size clientSize)
        {
            ViewpointInput.ClientSize = new Vector2(clientSize.Width, clientSize.Height);

            TimeManager.Tick();

            TimeSpan elapsed = TimeManager.DeltaTime;
            OnTick(elapsed);

            int subTickCount = (int)double.Ceiling(elapsed / LimitComputingTime);
            TimeSpan computeElapsed = elapsed / subTickCount;
            for (int i = 0; i < subTickCount; i++)
            {
                OnSubTick(computeElapsed);
            }

            OnDraw(clientSize);
        }

        protected virtual void OnSubTick(TimeSpan elapsed)
        {
            PhysicsHost.Simulation.Timestep((float)elapsed.TotalSeconds, PhysicsHost.ThreadDispatcher);
            World.SubTick(elapsed);
        }

        protected virtual void OnTick(TimeSpan elapsed)
        {
            string plateText = $"({Camera.PlateX}, {Camera.PlateZ})";
            string coordText = $"({Camera.PositionInWorld.X:F1}, {Camera.PositionInWorld.Y:F1}, {Camera.PositionInWorld.Z:F1})";
            Application.Current.MainWindow.Title = $"Bus {plateText}; {coordText} @ {TimeManager.Fps:f0} fps";

            World.Tick(elapsed);
        }

        protected virtual void OnDraw(System.Drawing.Size clientSize)
        {
            Renderer.Draw(Camera, World, clientSize);
        }

        public void OnKeyDown(Key key) => InputManager.OnKeyDown(key);
        public void OnKeyUp(Key key) => InputManager.OnKeyUp(key);
        public void OnMouseDragMove(System.Windows.Vector offset, MouseButtonState leftButton, MouseButtonState middleButton, MouseButtonState rightButton)
            => InputManager.OnMouseDragMove(offset, leftButton, middleButton, rightButton);
        public void OnMouseWheel(int delta) => InputManager.OnMouseWheel(delta);
    }
}
