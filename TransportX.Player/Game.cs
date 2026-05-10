using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Input;
using Silk.NET.SDL;
using Silk.NET.Windowing;

using TransportX.Dependency;
using TransportX.Diagnostics;
using TransportX.Worlds;

namespace TransportX.Player
{
    internal class Game : IDisposable
    {
        private readonly IWindow Window;
        private IInputContext? Input = null;

        private AppSession? Session = null;

        private bool IsReloadRequested = false;

        public Game()
        {
            WindowOptions options = WindowOptions.Default with
            {
                API = GraphicsAPI.None,
                VSync = false,
                Title = "TransportX",
            };
            Window = Silk.NET.Windowing.Window.Create(options);

            Window.Load += OnLoad;
            Window.Update += OnUpdate;
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

            IWorldInfo? worldInfo = WorldSelector.Select();
            if (worldInfo is null)
            {
                System.Environment.Exit(0);
                return;
            }

            bool isLoaded = LoadApp(worldInfo);
            if (!isLoaded)
            {
                System.Environment.Exit(0);
                return;
            }
        }

        private void OnUpdate(double deltaTime)
        {
            if (IsReloadRequested)
            {
                if (Session is null) throw new InvalidOperationException();

                IsReloadRequested = false;
                bool isLoaded = LoadApp(Session.WorldInfo);
                if (!isLoaded)
                {
                    System.Environment.Exit(0);
                    return;
                }
            }
        }

        private void OnClosing()
        {
            Session?.App.Dispose();
        }

        private bool LoadApp(IWorldInfo worldInfo)
        {
            if (Input is null) throw new InvalidOperationException();

            PluginLoadContext? oldAppContext = Session?.AppHost.Context;
            Session?.App.Dispose();
            oldAppContext?.Unload();
            Session = null;

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
            AppLoader loader = new(platform);

            try
            {
                (AppHost appHost, IApp app) = loader.Load(worldInfo);
                appHost.ReloadRequested += () => IsReloadRequested = true;
                Session = new(worldInfo, appHost, app);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "読込中にエラーが発生しました", MessageBoxFlags.Error);
                return false;
            }

            return true;
        }

        public void Run()
        {
            Window.Run();
        }


        private record AppSession(IWorldInfo WorldInfo, AppHost AppHost, IApp App);
    }
}
