using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.WIC;

namespace TransportX.Rendering.Importing
{
    internal class WICTextureFactory
    {
        private readonly ID3D11DeviceContext Context;
        private readonly IWICImagingFactory WICFactory;

        public WICTextureFactory(ID3D11DeviceContext context, IWICImagingFactory wicFactory)
        {
            Context = context;
            WICFactory = wicFactory;
        }

        public ID3D11ShaderResourceView Create(IWICBitmapDecoder decoder)
        {
            using IWICBitmapFrameDecode frame = decoder.GetFrame(0);
            IWICBitmapSource source = frame;

            IWICColorContext[]? colorContexts = null;
            IWICColorContext? destColorContext = null;
            IWICColorTransform? colorTransformer = null;

            try
            {
                colorContexts = frame.TryGetColorContexts(WICFactory);
                if (colorContexts is not null && 1 <= colorContexts.Length)
                {
                    destColorContext = WICFactory.CreateColorContext();
                    destColorContext.InitializeFromExifColorSpace(1); // sRGB

                    colorTransformer = WICFactory.CreateColorTransformer();
                    colorTransformer.Initialize(frame, colorContexts[0], destColorContext, PixelFormat.Format32bppBGRA);

                    source = colorTransformer;
                }

                using IWICFormatConverter converter = WICFactory.CreateFormatConverter();
                converter.Initialize(source, PixelFormat.Format32bppBGRA, BitmapDitherType.None, null, 0, BitmapPaletteType.Custom);

                int width = converter.Size.Width;
                int height = converter.Size.Height;

                uint stride = (uint)width * 4;
                uint bufferSize = stride * (uint)height;
                unsafe
                {
                    void* pBuffer = NativeMemory.Alloc(bufferSize);

                    try
                    {
                        converter.CopyPixels(stride, bufferSize, (nint)pBuffer);

                        Texture2DDescription desc = new()
                        {
                            Width = (uint)width,
                            Height = (uint)height,
                            MipLevels = 0,
                            ArraySize = 1,
                            Format = Format.B8G8R8A8_UNorm_SRgb,
                            SampleDescription = new SampleDescription(1, 0),
                            Usage = ResourceUsage.Default,
                            BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                            CPUAccessFlags = CpuAccessFlags.None,
                            MiscFlags = ResourceOptionFlags.GenerateMips,
                        };

                        using ID3D11Texture2D texture = Context.Device.CreateTexture2D(desc);
                        Context.UpdateSubresource(texture, 0, null, (nint)pBuffer, stride, bufferSize);

                        ID3D11ShaderResourceView view = Context.Device.CreateShaderResourceView(texture);
                        Context.GenerateMips(view);

                        return view;
                    }
                    finally
                    {
                        NativeMemory.Free(pBuffer);
                    }
                }
            }
            finally
            {
                destColorContext?.Dispose();
                colorTransformer?.Dispose();

                if (colorContexts is not null)
                {
                    foreach (IWICColorContext colorContext in colorContexts)
                    {
                        colorContext.Dispose();
                    }
                }
            }
        }

        public ID3D11ShaderResourceView CreateFromFile(string filePath)
        {
            using IWICBitmapDecoder decoder = WICFactory.CreateDecoderFromFileName(filePath);
            return Create(decoder);
        }

        public ID3D11ShaderResourceView CreateFromMemory(ReadOnlySpan<byte> data)
        {
            using IWICStream stream = WICFactory.CreateStream();
            stream.Initialize(data);
            using IWICBitmapDecoder decoder = WICFactory.CreateDecoderFromStream(stream);
            return Create(decoder);
        }
    }
}
