using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Windowing;
using Vortice.DXGI;
using Vortice.Mathematics;

using TransportX.Dependency;
using TransportX.Diagnostics;
using TransportX.Input;
using TransportX.Physics;
using TransportX.Rendering;

namespace TransportX.Worlds
{
    public class WorldAppFactory : IAppFactory<WorldAppParameters>
    {
        public IApp Create(IAppHost host, IAppParameters parameters)
        {
            if (parameters is not WorldAppParameters worldParameters)
            {
                throw new ArgumentException($"{nameof(parameters)} は {nameof(WorldAppParameters)} でなければなりません。", nameof(parameters));
            }

            return Create(host, worldParameters);
        }

        public IApp Create(IAppHost host, WorldAppParameters parameters)
        {
            DXHost dxHost = new();

            IWindow window = host.Platform.Window;
            if (window.Native is null || window.Native.Win32 is null)
            {
                throw new NotSupportedException("Windows 環境以外では実行できません。");
            }
            nint hwnd = window.Native.Win32.Value.Hwnd;

            SwapChainDescription1 swapChainDesc = new()
            {
                BufferCount = 2,
                Width = (uint)window.Size.X,
                Height = (uint)window.Size.Y,
                Format = Format.R8G8B8A8_UNorm,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.FlipDiscard,
                Scaling = Scaling.Stretch,
                BufferUsage = Usage.RenderTargetOutput,
            };
            SwapChainFullscreenDescription fullscreenDesc = new()
            {
                Windowed = true,
            };
            IDXGISwapChain1 swapChain = dxHost.DXGIFactory.CreateSwapChainForHwnd(dxHost.Device, hwnd, swapChainDesc, fullscreenDesc);

            DXClient dxClient = new(hwnd, swapChain);
            dxClient.Resize(dxHost.Device, window.Size.X, window.Size.Y);

            dxHost.Context.ClearRenderTargetView(dxClient.RenderTarget, new Color4(0, 0, 0));
            dxClient.SwapChain!.Present(1, PresentFlags.None);

            Renderer renderer = new(host.Platform, dxHost, dxClient);
            PhysicsHost physicsHost = PhysicsHost.Create();

            TimeManager updateTimeManager = new();
            TimeManager renderTimeManager = new();
            InputManager inputManager = new(host.Platform.Input);

            CameraLocation cameraLocation = new(0, 0, new Vector3(125, 10, 125), Vector2.Zero);
            try
            {
                Process process = Process.GetCurrentProcess();
                string savePath = Path.Combine(Path.GetDirectoryName(process.MainModule!.FileName)!, "Save.dat");

                string[] saveContent = File.ReadAllLines(savePath);

                if (int.Parse(saveContent[0]) == process.Id)
                {
                    string[] chunkText = saveContent[1].Split(',');
                    int chunkX = int.Parse(chunkText[0]);
                    int chunkZ = int.Parse(chunkText[1]);

                    string[] positionText = saveContent[2].Split(',');
                    Vector3 position = new(float.Parse(positionText[0]), float.Parse(positionText[1]), float.Parse(positionText[2]));

                    string[] angleText = saveContent[3].Split(',');
                    Vector2 angle = new(float.Parse(angleText[0]), float.Parse(angleText[1]));

                    cameraLocation = new(chunkX, chunkZ, position, angle);
                }
            }
            catch { }

            Camera camera = new(cameraLocation.ChunkX, cameraLocation.ChunkZ, cameraLocation.Position, cameraLocation.Angle);

            ErrorCollector errorCollector = new();
            WorldBuilder worldBuilder = new(parameters.WorldInfo)
            {
                Platform = host.Platform,
                DXHost = dxHost,
                DXClient = dxClient,
                PhysicsHost = physicsHost,
                ErrorCollector = errorCollector,
                AppContext = host.Context,
                TimeManager = updateTimeManager,
                InputManager = inputManager,
                Camera = camera,
            };
            WorldBase world = worldBuilder.Build();

            if (errorCollector.HasFatalError)
            {
                PluginLoadContext worldContext = world.WorldContext;
                world.Dispose();
                host.Context.Children.Remove(worldContext);
                worldContext.Unload();

                world = new EmptyWorld(worldBuilder);
            }

            WorldAppDependencies info = new()
            {
                Host = host,
                DXHost = dxHost,
                DXClient = dxClient,
                PhysicsHost = physicsHost,
                Renderer = renderer,
                UpdateTimeManager = updateTimeManager,
                RenderTimeManager = renderTimeManager,
                InputManager = inputManager,
                Camera = camera,
                World = world,
            };
            return new WorldApp(info);
        }


        private record CameraLocation(int ChunkX, int ChunkZ, Vector3 Position, Vector2 Angle);
    }
}
