using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Windowing;
using Vortice.DXGI;
using Vortice.Mathematics;

using TransportX.Cameras;
using TransportX.Data;
using TransportX.Dependency;
using TransportX.Diagnostics;
using TransportX.Input;
using TransportX.Physics;
using TransportX.Rendering;
using TransportX.Rendering.Shadows;

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
            ErrorCollector errorCollector = new();
            Config config = Config.Import(errorCollector);

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


            WorldOptions worldOptions = new()
            {
                SimulationChunkCount = config.SimulationChunkCount,
            };

            PhysicsHost physicsHost = PhysicsHost.Create();

            TimeManager updateTimeManager = new();
            TimeManager renderTimeManager = new();
            InputManager inputManager = new(host.Platform.Input);

            Camera camera = new();

            WorldBuilder worldBuilder = new(parameters.WorldInfo)
            {
                Platform = host.Platform,
                DXHost = dxHost,
                DXClient = dxClient,
                PhysicsHost = physicsHost,
                Options = worldOptions,
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


            RendererOptions rendererOptions = new()
            {
                DrawChunkCount = config.DrawChunkCount,
                ShadowOptions = new ShadowOptions()
                {
                    DrawChunkCount = config.ShadowDrawChunkCount,
                    Resolution = config.ShadowResolution,
                },
            };
            Renderer renderer = new(host.Platform, dxHost, dxClient, rendererOptions);

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
    }
}
