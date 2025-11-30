using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

using Vanara.PInvoke;
using Vortice.Direct3D11;
using Vortice.Direct3D11.Debug;
using Vortice.DXGI;

using Bus.Models;

namespace Bus.Components
{
    internal class D3D11Window : HwndHost
    {
        public static DependencyProperty DXHostProperty = DependencyProperty.Register(nameof(DXHost), typeof(DXHost), typeof(D3D11Window));
        public static DependencyProperty DXClientProperty = DependencyProperty.Register(nameof(DXClient), typeof(DXClient), typeof(D3D11Window));
        public static RoutedEvent RenderingEvent = EventManager.RegisterRoutedEvent(nameof(Rendering), RoutingStrategy.Bubble, typeof(RenderingEventHandler), typeof(D3D11Window));

        static D3D11Window()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(D3D11Window), new FrameworkPropertyMetadata(typeof(D3D11Window)));
        }


        private System.Drawing.Size Size;

        public DXHost? DXHost
        {
            get => GetValue(DXHostProperty) as DXHost;
            set => SetValue(DXHostProperty, value);
        }

        public DXClient? DXClient
        {
            get => GetValue(DXClientProperty) as DXClient;
            set => SetValue(DXClientProperty, value);
        }

        public event RenderingEventHandler Rendering
        {
            add => AddHandler(RenderingEvent, value);
            remove => RemoveHandler(RenderingEvent, value);
        }

        public D3D11Window() : base()
        {
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            if (DXHost is null) throw new InvalidOperationException();

            Size = new System.Drawing.Size(1, 1);

            ID3D11Debug? debug = DXHost.Device.CreationFlags.HasFlag(DeviceCreationFlags.Debug) ? DXHost.Device.QueryInterface<ID3D11Debug>() : null;

            User32.SafeHWND hwnd = User32.CreateWindowEx(
                lpClassName: "STATIC",
                dwStyle: User32.WindowStyles.WS_CHILD | User32.WindowStyles.WS_VISIBLE,
                nWidth: Size.Width, nHeight: Size.Height,
                hWndParent: hwndParent.Handle, hMenu: 2);

            DXHost.DXGIFactory.MakeWindowAssociation(hwnd.DangerousGetHandle(), WindowAssociationFlags.IgnoreAltEnter);

            SwapChainDescription1 swapChainDesc = new SwapChainDescription1()
            {
                BufferCount = 1,
                Width = (uint)Size.Width,
                Height = (uint)Size.Height,
                Format = Format.R8G8B8A8_UNorm,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Scaling = Scaling.Stretch,
                BufferUsage = Usage.RenderTargetOutput,
            };
            SwapChainFullscreenDescription fullscreenDesc = new SwapChainFullscreenDescription()
            {
                Windowed = true,
            };
            IDXGISwapChain1 swapChain = DXHost.DXGIFactory.CreateSwapChainForHwnd(DXHost.Device, hwnd.DangerousGetHandle(), swapChainDesc, fullscreenDesc);

            DXClient = new DXClient(hwnd.DangerousGetHandle(), swapChain, debug);

            System.Windows.Media.CompositionTarget.Rendering += OnCompositionTargetRendering;
            IsVisibleChanged += OnIsVisibleChanged;

            return new HandleRef(this, hwnd.DangerousGetHandle());
        }

        private void OnCompositionTargetRendering(object? sender, EventArgs e)
        {
            if (DXHost is null) throw new InvalidOperationException();
            if (DXClient is null) throw new InvalidOperationException();

            System.Drawing.Size newSize = new System.Drawing.Size((int)ActualWidth, (int)ActualHeight);
            if (Size != newSize)
            {
                DXClient.Resize(DXHost.Device, newSize.Width, newSize.Height);
                Size = newSize;
            }

            DXHost!.Context.OMSetRenderTargets(DXClient.RenderTarget!, DXClient.DepthStencil!);
            RaiseEvent(new RenderingEventArgs(RenderingEvent, DXClient.RenderTarget!, DXClient.DepthStencil!, Size));
            DXClient.SwapChain!.Present(0, PresentFlags.None);
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            System.Windows.Media.CompositionTarget.Rendering -= OnCompositionTargetRendering;
            IsVisibleChanged -= OnIsVisibleChanged;
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            DXClient?.Dispose();
        }
    }
}
