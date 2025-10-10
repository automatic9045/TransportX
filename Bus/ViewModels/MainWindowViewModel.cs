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

using Bus.Common.Rendering;
using Bus.Common.Worlds;

namespace Bus.ViewModels
{
    internal class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly CompositeDisposable Disposables = new CompositeDisposable();
        private readonly IRenderer Renderer;

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

            RendererLoader loader = new RendererLoader(dx);
            Renderer = loader.Load(worldInfo);

            KeyDownCommand = new ReactiveCommandSlim<KeyEventArgs>().AddTo(Disposables);
            KeyUpCommand = new ReactiveCommandSlim<KeyEventArgs>().AddTo(Disposables);
            MouseWheelCommand = new ReactiveCommandSlim<MouseWheelEventArgs>().AddTo(Disposables);
            RenderingCommand = new ReactiveCommandSlim<RenderingEventArgs>().AddTo(Disposables);

            MouseDragOffset.Subscribe(Renderer.OnMouseDragMove);
            KeyDownCommand.Subscribe(args => Renderer.OnKeyDown(args.Key));
            KeyUpCommand.Subscribe(args => Renderer.OnKeyUp(args.Key));
            MouseWheelCommand.Subscribe(args => Renderer.OnMouseWheel(args.Delta));
            RenderingCommand.Subscribe(args => Renderer.Draw(args.RenderTarget, args.DepthStencil, args.Size));

            Application.Current.Exit += (sender, e) =>
            {
                Renderer.Dispose();
                dx.Dispose();
            };
        }

        public void Dispose()
        {
            Disposables.Dispose();
        }
    }
}
