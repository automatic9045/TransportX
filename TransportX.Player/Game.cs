using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.SDL;
using Silk.NET.Windowing;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

using TransportX.Dependency;
using TransportX.Diagnostics;
using TransportX.Worlds;

namespace TransportX.Player
{
    internal class Game : IDisposable
    {
        private readonly DXHost DXHost;
        private readonly IWindow Window;

        private DXClient? DXClient = null;
        private IInputContext? Input = null;

        private IWorldInfo? WorldInfo = null;
        private IRuntime? Runtime = null;

        public Game()
        {
            DXHost = new DXHost();

            WindowOptions options = WindowOptions.Default with
            {
                API = GraphicsAPI.None,
                VSync = false,
                Title = "TransportX",
            };
            Window = Silk.NET.Windowing.Window.Create(options);

            Window.Load += OnLoad;
            Window.Update += OnUpdate;
            Window.Render += OnRender;
            Window.Resize += OnResize;
            Window.Closing += OnClosing;

            AssemblyLoadContext.Default.Resolving += (context, name) =>
            {
                HostAssemblyLoader.TryLoad(name, out Assembly? assembly);
                return assembly;
            };
        }

        public void Dispose()
        {
            Input?.Dispose();
            Window.Dispose();
        }

        private void OnLoad()
        {
            Input = Window.CreateInput();

            if (Window.Native is null || Window.Native.Win32 is null)
            {
                throw new NotSupportedException("Windows 環境以外では実行できません。");
            }
            nint hwnd = Window.Native.Win32.Value.Hwnd;

            SwapChainDescription1 swapChainDesc = new()
            {
                BufferCount = 2,
                Width = (uint)Window.Size.X,
                Height = (uint)Window.Size.Y,
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
            IDXGISwapChain1 swapChain = DXHost.DXGIFactory.CreateSwapChainForHwnd(DXHost.Device, hwnd, swapChainDesc, fullscreenDesc);

            DXClient = new DXClient(hwnd, swapChain);
            DXClient.Resize(DXHost.Device, Window.Size.X, Window.Size.Y);

            IWorldInfo? worldInfo = WorldSelector.Select();
            if (worldInfo is null)
            {
                System.Environment.Exit(0);
                return;
            }

            bool isLoaded = LoadRuntime(worldInfo);
            if (!isLoaded)
            {
                System.Environment.Exit(0);
                return;
            }
        }

        private void OnUpdate(double deltaTime)
        {
            if (Input is null || WorldInfo is null) throw new InvalidOperationException();

            for (int i = 0; i < Input.Keyboards.Count; i++)
            {
                if (Input.Keyboards[i].IsKeyPressed(Key.F5))
                {
                    bool isLoaded = LoadRuntime(WorldInfo);
                    if (!isLoaded)
                    {
                        System.Environment.Exit(0);
                        return;
                    }
                    break;
                }
            }
        }

        private void OnRender(double deltaTime)
        {
            if (DXClient is null || DXClient.RenderTarget is null) throw new InvalidOperationException();

            if (Runtime is null)
            {
                ID3D11DeviceContext context = DXHost.Context;

                context.OMSetRenderTargets(DXClient.RenderTarget, DXClient.DepthStencil);
                Viewport viewport = new(0, 0, Window.Size.X, Window.Size.Y);
                context.RSSetViewport(viewport);

                context.ClearRenderTargetView(DXClient.RenderTarget, new Color4(0, 0, 0, 1));
                if (DXClient.DepthStencil is not null)
                {
                    context.ClearDepthStencilView(DXClient.DepthStencil, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
                }
            }
            else
            {
                Runtime.Draw();
            }

            DXClient.SwapChain!.Present(Window.VSync ? 1u : 0u, PresentFlags.None);
        }

        private void OnResize(Vector2D<int> size)
        {
            if (DXClient is null) throw new InvalidOperationException();

            if (0 < size.X && 0 < size.Y)
            {
                DXClient.Resize(DXHost.Device, size.X, size.Y);
            }
        }

        private void OnClosing()
        {
            Runtime?.Dispose();
            DXClient?.Dispose();
            DXHost.Dispose();
        }

        private bool LoadRuntime(IWorldInfo worldInfo)
        {
            if (DXClient is null || DXClient.RenderTarget is null || Input is null) throw new InvalidOperationException();

            WorldInfo = worldInfo;

            DXHost.Context.ClearRenderTargetView(DXClient.RenderTarget, Colors.Blue);
            DXClient.SwapChain.Present(0);

            PluginLoadContext? oldRuntimeContext = Runtime?.Context;
            Runtime?.Dispose();
            oldRuntimeContext?.Unload();
            Runtime = null;

            // !!! .NET Runtime のバグ回避のため !!!
            // 参考: https://github.com/dotnet/runtime/issues/123930
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                for (int i = 0; i < 3; i++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }

            Platform platform = new()
            {
                Window = Window,
                Input = Input,
            };
            RuntimeLoader loader = new(platform, DXHost, DXClient);

            try
            {
                Runtime = loader.Load(WorldInfo);
            }
            catch (Exception ex)
            {
                
                MessageBox.Show(ex.ToString(), "読込中にエラーが発生しました", MessageBoxFlags.Error);

                DXHost.Context.ClearRenderTargetView(DXClient.RenderTarget, Colors.Black);
                DXClient.SwapChain.Present(0);
                return false;
            }

            return true;
        }

        public void Run()
        {
            Window.Run();
        }
    }
}
