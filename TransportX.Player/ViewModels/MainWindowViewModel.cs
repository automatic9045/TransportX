using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Reactive.Bindings;
using Reactive.Bindings.Disposables;
using Reactive.Bindings.Extensions;
using Vortice.Mathematics;

using TransportX.Components;
using TransportX.Models;

using TransportX.Dependency;
using TransportX.Worlds;

namespace TransportX.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly CompositeDisposable Disposables = new CompositeDisposable();

        private IWorldInfo? WorldInfo = null;
        private IRuntime? Runtime = null;
        private CompositeDisposable? PerRuntimeDisposables = null;

        public ReactivePropertySlim<DXHost> DXHost { get; }
        public ReactivePropertySlim<DXClient?> DXClient { get; }

        public ReactivePropertySlim<Vector> MouseDragOffset { get; }
        public ReactivePropertySlim<MouseButtonState> MouseLeftButton { get; }
        public ReactivePropertySlim<MouseButtonState> MouseMiddleButton { get; }
        public ReactivePropertySlim<MouseButtonState> MouseRightButton { get; }

        public ReactiveCommandSlim<KeyEventArgs> KeyDownCommand { get; }
        public ReactiveCommandSlim<KeyEventArgs> KeyUpCommand { get; }
        public ReactiveCommandSlim<MouseWheelEventArgs> MouseWheelCommand { get; }
        public ReactiveCommandSlim<RenderingEventArgs> RenderingCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindowViewModel()
        {
            DXHost dx = new DXHost();

            DXHost = new ReactivePropertySlim<DXHost>(dx).AddTo(Disposables);
            DXClient = new ReactivePropertySlim<DXClient?>().AddTo(Disposables);

            MouseDragOffset = new ReactivePropertySlim<Vector>(mode: ReactivePropertyMode.None).AddTo(Disposables);
            MouseLeftButton = new ReactivePropertySlim<MouseButtonState>(mode: ReactivePropertyMode.None).AddTo(Disposables);
            MouseMiddleButton = new ReactivePropertySlim<MouseButtonState>(mode: ReactivePropertyMode.None).AddTo(Disposables);
            MouseRightButton = new ReactivePropertySlim<MouseButtonState>(mode: ReactivePropertyMode.None).AddTo(Disposables);

            KeyDownCommand = new ReactiveCommandSlim<KeyEventArgs>().AddTo(Disposables);
            KeyUpCommand = new ReactiveCommandSlim<KeyEventArgs>().AddTo(Disposables);
            MouseWheelCommand = new ReactiveCommandSlim<MouseWheelEventArgs>().AddTo(Disposables);
            RenderingCommand = new ReactiveCommandSlim<RenderingEventArgs>().AddTo(Disposables);

            KeyDownCommand.Subscribe(e =>
            {
                if (e.Key == Key.F5)
                {
                    if (WorldInfo is not null) LoadRuntime(WorldInfo);
                }
            }).AddTo(Disposables);

            Application.Current.Exit += (sender, e) =>
            {
                PerRuntimeDisposables?.Dispose();
                dx.Dispose();
            };

            AssemblyLoadContext.Default.Resolving += (context, name) =>
            {
                HostAssemblyLoader.TryLoad(name, out Assembly? assembly);
                return assembly;
            };
        }

        public void Dispose()
        {
            Disposables.Dispose();
        }

        internal bool LoadRuntime(IWorldInfo worldInfo)
        {
            WorldInfo = worldInfo;
            Application.Current.MainWindow.Title = "Bus - Now loading...";

            DXHost dxHost = DXHost.Value;
            DXClient dxClient = DXClient.Value!;

            dxHost.Context.ClearRenderTargetView(dxClient.RenderTarget, Colors.Blue);
            dxClient.SwapChain.Present(0);

            PluginLoadContext? oldRuntimeContext = Runtime?.Context;
            PerRuntimeDisposables?.Dispose();
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

            RuntimeLoader loader = new RuntimeLoader(dxHost, dxClient);
            IRuntime runtime;
            try
            {
                runtime = loader.Load(WorldInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "読込中にエラーが発生しました", MessageBoxButton.OK, MessageBoxImage.Error);

                Application.Current.MainWindow.Title = "Bus";
                dxHost.Context.ClearRenderTargetView(dxClient.RenderTarget, Colors.Black);
                dxClient.SwapChain.Present(0);
                return false;
            }

            PerRuntimeDisposables = new CompositeDisposable();
            Runtime = runtime.AddTo(PerRuntimeDisposables);

            Observable.CombineLatest(MouseDragOffset, MouseLeftButton, MouseMiddleButton, MouseRightButton,
                (o, l, m, r) => (Offset: o, Left: l, Middle: m, Right: r))
                .Subscribe(t => Runtime.OnMouseDragMove(t.Offset, t.Left, t.Middle, t.Right)).AddTo(PerRuntimeDisposables);
            KeyDownCommand.Subscribe(args => Runtime.OnKeyDown(args.Key)).AddTo(PerRuntimeDisposables);
            KeyUpCommand.Subscribe(args => Runtime.OnKeyUp(args.Key)).AddTo(PerRuntimeDisposables);
            MouseWheelCommand.Subscribe(args => Runtime.OnMouseWheel(args.Delta)).AddTo(PerRuntimeDisposables);
            RenderingCommand.Subscribe(args => Runtime.Draw(args.Size)).AddTo(PerRuntimeDisposables);

            return true;
        }
    }
}
