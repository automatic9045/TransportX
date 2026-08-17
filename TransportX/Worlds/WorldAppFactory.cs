using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Windowing;
using Vortice.DXGI;
using Vortice.Mathematics;

using TransportX.Audio;
using TransportX.Cameras;
using TransportX.Data;
using TransportX.Dependency;
using TransportX.Diagnostics;
using TransportX.Input;
using TransportX.Physics;
using TransportX.Rendering.Backend;
using TransportX.Rendering.Pipelines;

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
            IWindow window = host.Platform.Window;
            if (window.Native is null || window.Native.Win32 is null)
            {
                throw new NotSupportedException("Windows 環境以外では実行できません。");
            }
            nint hwnd = window.Native.Win32.Value.Hwnd;

            ErrorCollector errorCollector = new(host.Platform.Window);
            Config config = Config.Import(errorCollector);

            GraphicsHost graphicsHost = new();

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
            IDXGISwapChain1 swapChain = graphicsHost.DXGIFactory.CreateSwapChainForHwnd(graphicsHost.Device, hwnd, swapChainDesc, fullscreenDesc);

            GraphicsClient graphicsClient = new(hwnd, swapChain);
            graphicsClient.Resize(graphicsHost.Device, window.Size.X, window.Size.Y);

            graphicsHost.Context.ClearRenderTargetView(graphicsClient.Surface?.RenderTarget, new Color4(0, 0, 0));
            graphicsClient.SwapChain!.Present(1, PresentFlags.None);

            AudioHost audioHost = new();
            AudioClient audioClient = new();

            PhysicsHost physicsHost = PhysicsHost.Create();

            WorldOptions worldOptions = new()
            {
                SimulationChunkCount = config.SimulationChunkCount,
                IsDebugMode = config.IsDebugMode,
            };

            TimeManager updateTimeManager = new();
            TimeManager renderTimeManager = new();
            InputManager inputManager = new(host.Platform.Input);

            Camera camera = new();

            WorldBuilder worldBuilder = new(parameters.WorldInfo)
            {
                Platform = host.Platform,
                GraphicsHost = graphicsHost,
                GraphicsClient = graphicsClient,
                AudioHost = audioHost,
                AudioClient = audioClient,
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
            Renderer renderer = new(host.Platform, graphicsHost, graphicsClient, rendererOptions);

            WorldAppDependencies info = new()
            {
                Host = host,
                GraphicsHost = graphicsHost,
                GraphicsClient = graphicsClient,
                AudioClient = audioClient,
                PhysicsHost = physicsHost,
                Viewpoints = new ViewpointSet(),
                Renderer = renderer,
                UpdateTimeManager = updateTimeManager,
                RenderTimeManager = renderTimeManager,
                World = world,
            };
            return new WorldApp(info);
        }
    }
}
