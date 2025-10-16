using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Reactive.Bindings;
using Reactive.Bindings.Disposables;
using Reactive.Bindings.Extensions;

using Bus.Components;
using Bus.Models;

using Bus.Common;
using Bus.Common.Worlds;

namespace Bus.ViewModels
{
    internal class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly CompositeDisposable Disposables = new CompositeDisposable();
        private readonly IGame Game;

        public ReactivePropertySlim<DXHost> DXHost { get; }
        public ReactivePropertySlim<Vector> MouseDragOffset { get; }

        public ReactiveCommandSlim<KeyEventArgs> KeyDownCommand { get; }
        public ReactiveCommandSlim<KeyEventArgs> KeyUpCommand { get; }
        public ReactiveCommandSlim<MouseWheelEventArgs> MouseWheelCommand { get; }
        public ReactiveCommandSlim<RenderingEventArgs> RenderingCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindowViewModel(IWorldInfo worldInfo)
        {
            DXHost dx = new DXHost();

            DXHost = new ReactivePropertySlim<DXHost>(dx).AddTo(Disposables);
            MouseDragOffset = new ReactivePropertySlim<Vector>(mode: ReactivePropertyMode.None).AddTo(Disposables);

            GameLoader loader = new GameLoader(dx);
            Game = loader.Load(worldInfo);

            KeyDownCommand = new ReactiveCommandSlim<KeyEventArgs>().AddTo(Disposables);
            KeyUpCommand = new ReactiveCommandSlim<KeyEventArgs>().AddTo(Disposables);
            MouseWheelCommand = new ReactiveCommandSlim<MouseWheelEventArgs>().AddTo(Disposables);
            RenderingCommand = new ReactiveCommandSlim<RenderingEventArgs>().AddTo(Disposables);

            MouseDragOffset.Subscribe(Game.OnMouseDragMove);
            KeyDownCommand.Subscribe(args => Game.OnKeyDown(args.Key));
            KeyUpCommand.Subscribe(args => Game.OnKeyUp(args.Key));
            MouseWheelCommand.Subscribe(args => Game.OnMouseWheel(args.Delta));
            RenderingCommand.Subscribe(args => Game.Draw(args.RenderTarget, args.DepthStencil, args.Size));

            Application.Current.Exit += (sender, e) =>
            {
                Game.Dispose();
                dx.Dispose();
            };
        }

        public void Dispose()
        {
            Disposables.Dispose();
        }
    }
}
