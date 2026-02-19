using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

namespace TransportX.Rendering
{
    public class RenderTexture : IDisposable
    {
        public Size Size { get; }
        public ID3D11Texture2D Texture { get; }
        public ID3D11RenderTargetView RenderTargetView { get; }
        public ID3D11ShaderResourceView ShaderResourceView { get; }

        public RenderTexture(ID3D11Device device, Texture2DDescription description)
        {
            Size = new Size((int)description.Width, (int)description.Height);
            Texture = device.CreateTexture2D(description);
            RenderTargetView = device.CreateRenderTargetView(Texture);
            ShaderResourceView = device.CreateShaderResourceView(Texture);
        }

        public void Dispose()
        {
            ShaderResourceView.Dispose();
            RenderTargetView.Dispose();
            Texture.Dispose();
        }
    }
}
