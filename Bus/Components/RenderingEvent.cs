using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Vortice.Direct3D11;

namespace Bus.Components
{
    internal delegate void RenderingEventHandler(object sender, RenderingEventArgs e);

    public class RenderingEventArgs : RoutedEventArgs
    {
        public ID3D11RenderTargetView RenderTarget { get; }
        public ID3D11DepthStencilView DepthStencil { get; }
        public System.Drawing.Size Size { get; }

        public RenderingEventArgs(RoutedEvent routedEvent, ID3D11RenderTargetView renderTarget, ID3D11DepthStencilView depthStencil, System.Drawing.Size size) : base(routedEvent)
        {
            RenderTarget = renderTarget;
            DepthStencil = depthStencil;
            Size = size;
        }
    }
}
