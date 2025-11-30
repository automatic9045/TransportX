using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Reactive.Bindings;
using Reactive.Bindings.Disposables;
using Reactive.Bindings.Extensions;
using Vortice.Direct3D11;
using Vortice.Mathematics;

using Bus.Components;
using Bus.Models;

using Bus.Common;
using Bus.Common.Worlds;

namespace Bus.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly CompositeDisposable Disposables = new CompositeDisposable();

        private IWorldInfo? WorldInfo = null;
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

            Application.Current.Exit += (sender, e) =>
            {
                PerGameDisposables?.Dispose();
                dx.Dispose();
            };

            KeyDownCommand.Subscribe(e =>
            {
                if (e.Key == Key.F5)
                {
                    if (WorldInfo is not null) LoadGame(WorldInfo);
                }
            }).AddTo(Disposables);
        }

        public void Dispose()
        {
            Disposables.Dispose();
        }

        internal void LoadGame(IWorldInfo worldInfo)
        {
            WorldInfo = worldInfo;
            Application.Current.MainWindow.Title = "Bus - Now loading...";

            DXHost dxHost = DXHost.Value;
            DXClient dxClient = DXClient.Value!;

            dxHost.Context.ClearRenderTargetView(dxClient.RenderTarget, Colors.Blue);
            dxClient.SwapChain.Present(0);

            PerGameDisposables?.Dispose();
            PerGameDisposables = new CompositeDisposable();

            GameLoader loader = new GameLoader(dxHost, dxClient);
            IGame game = loader.Load(worldInfo).AddTo(PerGameDisposables);

            Observable.CombineLatest(MouseDragOffset, MouseLeftButton, MouseMiddleButton, MouseRightButton,
                (o, l, m, r) => (Offset: o, Left: l, Middle: m, Right: r))
                .Subscribe(t => game.OnMouseDragMove(t.Offset, t.Left, t.Middle, t.Right)).AddTo(PerGameDisposables);
            KeyDownCommand.Subscribe(args => game.OnKeyDown(args.Key)).AddTo(PerGameDisposables);
            KeyUpCommand.Subscribe(args => game.OnKeyUp(args.Key)).AddTo(PerGameDisposables);
            MouseWheelCommand.Subscribe(args => game.OnMouseWheel(args.Delta)).AddTo(PerGameDisposables);
            RenderingCommand.Subscribe(args => game.Draw(args.RenderTarget, args.DepthStencil, args.Size)).AddTo(PerGameDisposables);
        }
    }
}
