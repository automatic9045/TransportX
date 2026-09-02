using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;
using Vortice.Mathematics;

namespace TransportX.Rendering.Backend
{
    public class DepthTexture : IDisposable
    {
        public SizeI Size { get; }
        public ID3D11Texture2D Texture { get; }
        public ID3D11DepthStencilView DepthStencilView { get; }

        public DepthTexture(ID3D11Device device, Texture2DDescription description)
        {
            Size = new SizeI((int)description.Width, (int)description.Height);
            Texture = device.CreateTexture2D(description);
            DepthStencilView = device.CreateDepthStencilView(Texture);
        }

        public void Dispose()
        {
            DepthStencilView.Dispose();
            Texture.Dispose();
        }
    }
}
