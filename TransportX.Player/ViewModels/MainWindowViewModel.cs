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

using TransportX;
using TransportX.Dependency;
using TransportX.Worlds;

namespace TransportX.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly CompositeDisposable Disposables = new CompositeDisposable();

        private IWorldInfo? WorldInfo = null;
        private IGame? Game = null;
        private CompositeDisposable? PerGameDisposables = null;

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
                    if (WorldInfo is not null) LoadGame(WorldInfo);
                }
            }).AddTo(Disposables);

            Application.Current.Exit += (sender, e) =>
            {
                PerGameDisposables?.Dispose();
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

        internal bool LoadGame(IWorldInfo worldInfo)
        {
            WorldInfo = worldInfo;
            Application.Current.MainWindow.Title = "Bus - Now loading...";

            DXHost dxHost = DXHost.Value;
            DXClient dxClient = DXClient.Value!;

            dxHost.Context.ClearRenderTargetView(dxClient.RenderTarget, Colors.Blue);
            dxClient.SwapChain.Present(0);

            PluginLoadContext? oldGameContext = Game?.Context;
            PerGameDisposables?.Dispose();
            oldGameContext?.Unload();
            Game = null;
            GC.Collect();

            GameLoader loader = new GameLoader(dxHost, dxClient);
            IGame game;
            try
            {
                game = loader.Load(WorldInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "読込中にエラーが発生しました", MessageBoxButton.OK, MessageBoxImage.Error);

                Application.Current.MainWindow.Title = "Bus";
                dxHost.Context.ClearRenderTargetView(dxClient.RenderTarget, Colors.Black);
                dxClient.SwapChain.Present(0);
                return false;
            }

            PerGameDisposables = new CompositeDisposable();
            Game = game.AddTo(PerGameDisposables);

            Observable.CombineLatest(MouseDragOffset, MouseLeftButton, MouseMiddleButton, MouseRightButton,
                (o, l, m, r) => (Offset: o, Left: l, Middle: m, Right: r))
                .Subscribe(t => Game.OnMouseDragMove(t.Offset, t.Left, t.Middle, t.Right)).AddTo(PerGameDisposables);
            KeyDownCommand.Subscribe(args => Game.OnKeyDown(args.Key)).AddTo(PerGameDisposables);
            KeyUpCommand.Subscribe(args => Game.OnKeyUp(args.Key)).AddTo(PerGameDisposables);
            MouseWheelCommand.Subscribe(args => Game.OnMouseWheel(args.Delta)).AddTo(PerGameDisposables);
            RenderingCommand.Subscribe(args => Game.Draw(args.Size)).AddTo(PerGameDisposables);

            return true;
        }
    }
}
