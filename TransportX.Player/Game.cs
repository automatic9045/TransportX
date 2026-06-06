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
using Silk.NET.Windowing.Sdl;

using TransportX.Dependency;
using TransportX.Diagnostics;

using TransportX.Player.Launcher;

namespace TransportX.Player
{
    internal class Game : IDisposable
    {
        private readonly IWindow Window;
        private IInputContext? Input = null;

        private AppRequest? Request = null;
        private AppSession? Session = null;

        public Game()
        {
            SdlWindowing.Use();

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
            try
            {
                Window.Dispose();
            }
            catch (InvalidOperationException) { } // OnRender 中に例外がスローされ、キャッチされなかった場合、Window.Dispose にてスローされた例外で上書きされてしまうため
        }

        private void OnLoad()
        {
            Input = Window.CreateInput();

            AppReference reference = AppReference.FromType<LauncherApp.Factory>();
            Request = new AppRequest(reference, IAppParameters.Empty);
        }

        private void OnUpdate(double deltaTime)
        {
            if (Request is not null)
            {
                bool isLoaded = LoadApp(Request.Reference, Request.Parameters);
                Request = null;

                if (!isLoaded)
                {
                    System.Environment.Exit(0);
                    return;
                }
            }


            bool LoadApp(AppReference reference, IAppParameters parameters)
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
                    (AppHost appHost, IApp app) = loader.Load(reference, parameters);
                    appHost.LoadRequested += (reference, parameters) => Request = new(reference, parameters);
                    Session = new(appHost, app);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "アプリケーション読込中にエラーが発生しました", MessageBoxFlags.Error);
                    return false;
                }

                return true;
            }
        }

        private void OnClosing()
        {
            Session?.App.Dispose();
            Session = null;
        }

        public void Run()
        {
            Window.Run();
        }


        private record AppRequest(AppReference Reference, IAppParameters Parameters);
        private record AppSession(AppHost AppHost, IApp App);
    }
}
