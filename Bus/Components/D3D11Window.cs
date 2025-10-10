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
        public static RoutedEvent RenderingEvent = EventManager.RegisterRoutedEvent(nameof(Rendering), RoutingStrategy.Bubble, typeof(RenderingEventHandler), typeof(D3D11Window));

        static D3D11Window()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(D3D11Window), new FrameworkPropertyMetadata(typeof(D3D11Window)));
        }


        private readonly bool IsInDesignMode;

        private ID3D11Debug? D3DDebug;
        private User32.SafeHWND? Hwnd;
        private IDXGISwapChain? SwapChain;
        private ID3D11RenderTargetView? RenderTarget;
        private ID3D11DepthStencilView? DepthStencil;

        private System.Drawing.Size Size;

        public DXHost? DXHost
        {
            get => GetValue(DXHostProperty) as DXHost;
            set => SetValue(DXHostProperty, value);
        }

        public event RenderingEventHandler Rendering
        {
            add => AddHandler(RenderingEvent, value);
            remove => RemoveHandler(RenderingEvent, value);
        }

        public D3D11Window() : base()
        {
            IsInDesignMode = DesignerProperties.GetIsInDesignMode(this);
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            if (DXHost is null) throw new InvalidOperationException();

            Size = new System.Drawing.Size(1, 1);

            if (DXHost.Device.CreationFlags.HasFlag(DeviceCreationFlags.Debug)) D3DDebug = DXHost.Device.QueryInterface<ID3D11Debug>();

            Hwnd = User32.CreateWindowEx(
                lpClassName: "STATIC",
                dwStyle: User32.WindowStyles.WS_CHILD | User32.WindowStyles.WS_VISIBLE,
                nWidth: Size.Width, nHeight: Size.Height,
                hWndParent: hwndParent.Handle, hMenu: 2);

            DXHost.DXGIFactory.MakeWindowAssociation(Hwnd.DangerousGetHandle(), WindowAssociationFlags.IgnoreAltEnter);

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
            SwapChain = DXHost.DXGIFactory.CreateSwapChainForHwnd(DXHost.Device, Hwnd.DangerousGetHandle(), swapChainDesc, fullscreenDesc);

            System.Windows.Media.CompositionTarget.Rendering += OnCompositionTargetRendering;
            IsVisibleChanged += OnIsVisibleChanged;

            return new HandleRef(this, Hwnd.DangerousGetHandle());
        }

        private void OnCompositionTargetRendering(object? sender, EventArgs e)
        {
            if (DXHost is null) throw new InvalidOperationException();

            System.Drawing.Size newSize = new System.Drawing.Size((int)ActualWidth, (int)ActualHeight);
            if (Size != newSize)
            {
                RenderTarget?.Dispose();
                DepthStencil?.Dispose();

                SwapChain!.ResizeBuffers(1, (uint)newSize.Width, (uint)newSize.Height);

                using (ID3D11Texture2D backBuffer = SwapChain!.GetBuffer<ID3D11Texture2D>(0))
                {
                    RenderTarget = DXHost!.Device.CreateRenderTargetView(backBuffer);
                }

                Texture2DDescription depthBufferDesc = new Texture2DDescription()
                {
                    Format = Format.D32_Float_S8X24_UInt,
                    ArraySize = 1,
                    MipLevels = 1,
                    Width = (uint)newSize.Width,
                    Height = (uint)newSize.Height,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.DepthStencil,
                    CPUAccessFlags = CpuAccessFlags.None,
                    MiscFlags = ResourceOptionFlags.None,
                };
                using (ID3D11Texture2D depthBuffer = DXHost.Device.CreateTexture2D(depthBufferDesc))
                {
                    DepthStencilViewDescription depthStencilDesc = new DepthStencilViewDescription()
                    {
                        Format = depthBufferDesc.Format,
                        ViewDimension = DepthStencilViewDimension.Texture2D,
                    };
                    depthStencilDesc.Texture2D.MipSlice = 0;
                    DepthStencil = DXHost.Device.CreateDepthStencilView(depthBuffer, depthStencilDesc);
                }

                Size = newSize;
            }

            DXHost!.Context.OMSetRenderTargets(RenderTarget!, DepthStencil!);
            RaiseEvent(new RenderingEventArgs(RenderingEvent, RenderTarget!, DepthStencil!, Size));
            SwapChain!.Present(0, PresentFlags.None);
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            System.Windows.Media.CompositionTarget.Rendering -= OnCompositionTargetRendering;
            IsVisibleChanged -= OnIsVisibleChanged;
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            RenderTarget?.Dispose();
            DepthStencil?.Dispose();
            SwapChain?.Dispose();
            User32.DestroyWindow(hwnd.Handle);

            D3DDebug?.ReportLiveDeviceObjects(ReportLiveDeviceObjectFlags.Detail);
            D3DDebug?.Dispose();
        }
    }
}
