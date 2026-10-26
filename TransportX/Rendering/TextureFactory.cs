using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;
using Vortice.WIC;

using TransportX.Diagnostics;
using TransportX.Rendering.Importing;

namespace TransportX.Rendering
{
    public class TextureFactory
    {
        private readonly WICTextureFactory WICFactory;
        private readonly DDSTextureFactory DDSFactory;

        public TextureFactory(ID3D11DeviceContext context, IWICImagingFactory wic)
        {
            WICFactory = new WICTextureFactory(context, wic);
            DDSFactory = new DDSTextureFactory(context.Device);
        }

        public ID3D11ShaderResourceView CreateFromFile(string filePath, bool isLinear, IErrorCollector errorCollector)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            ID3D11ShaderResourceView shaderResourceView = extension switch
            {
                ".dds" => DDSFactory.CreateFromFile(filePath),
                _ => WICFactory.CreateFromFile(filePath, isLinear, errorCollector),
            };
            return shaderResourceView;
        }
    }
}
