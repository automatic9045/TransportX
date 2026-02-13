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
        private readonly IWICImagingFactory WIC;

        public WICTextureFactory(ID3D11DeviceContext context, IWICImagingFactory wic)
        {
            Context = context;
            WIC = wic;
        }

        public ID3D11ShaderResourceView Create(IWICBitmapDecoder decoder, bool isLinear)
        {
            using IWICBitmapFrameDecode frame = decoder.GetFrame(0);
            IWICBitmapSource source = frame;

            IWICColorContext[]? colorContexts = null;
            IWICColorContext? destColorContext = null;
            IWICColorTransform? colorTransformer = null;

            try
            {
                colorContexts = frame.TryGetColorContexts(WIC);
                if (!isLinear && colorContexts is not null && 1 <= colorContexts.Length)
                {
                    destColorContext = WIC.CreateColorContext();
                    destColorContext.InitializeFromExifColorSpace(1); // sRGB

                    colorTransformer = WIC.CreateColorTransformer();
                    colorTransformer.Initialize(frame, colorContexts[0], destColorContext, PixelFormat.Format32bppBGRA);

                    source = colorTransformer;
                }

                using IWICFormatConverter converter = WIC.CreateFormatConverter();
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
                            Format = isLinear ? Format.B8G8R8A8_UNorm : Format.B8G8R8A8_UNorm_SRgb,
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

        public ID3D11ShaderResourceView CreateFromFile(string filePath, bool isLinear)
        {
            using IWICBitmapDecoder decoder = WIC.CreateDecoderFromFileName(filePath);
            return Create(decoder, isLinear);
        }

        public ID3D11ShaderResourceView CreateFromMemory(ReadOnlySpan<byte> data, bool isLinear)
        {
            using IWICStream stream = WIC.CreateStream();
            stream.Initialize(data);
            using IWICBitmapDecoder decoder = WIC.CreateDecoderFromStream(stream);
            return Create(decoder, isLinear);
        }

        public unsafe ID3D11ShaderResourceView CreateFromMerged(IWICStream? rStream, IWICStream? gStream, IWICStream? bStream, bool isLinear)
        {
            using IWICBitmapSource bSource = bStream is null ? CreateSingleBitmap(255, 255, 255, 255) : LoadBitmap(bStream);
            using IWICBitmapSource gSource = gStream is null ? CreateSingleBitmap(255, 255, 255, 255) : LoadBitmap(gStream);
            using IWICBitmapSource rSource = rStream is null ? CreateSingleBitmap(255, 255, 255, 255) : LoadBitmap(rStream);

            IWICBitmapSource LoadBitmap(IWICStream stream)
            {
                using IWICBitmapDecoder decoder = WIC.CreateDecoderFromStream(stream);
                using IWICBitmapFrameDecode frame = decoder.GetFrame(0);

                return WIC.CreateBitmapFromSource(frame, BitmapCreateCacheOption.CacheOnLoad);
            }

            IWICBitmapSource CreateSingleBitmap(byte r, byte g, byte b, byte a)
            {
                byte[] pixel = [b, g, r, a];
                fixed (byte* p = pixel)
                {
                    return WIC.CreateBitmapFromMemory(1, 1, PixelFormat.Format32bppBGRA, 4, 4, (nint)p);
                }
            }


            uint width = (uint)int.Max(bSource.Size.Width, int.Max(gSource.Size.Width, rSource.Size.Width));
            uint height = (uint)int.Max(bSource.Size.Height, int.Max(gSource.Size.Height, rSource.Size.Height));


            using IWICBitmapSource bResized = ConvertAndResize(bSource, width, height);
            using IWICBitmapSource gResized = ConvertAndResize(gSource, width, height);
            using IWICBitmapSource rResized = ConvertAndResize(rSource, width, height);

            IWICBitmapSource ConvertAndResize(IWICBitmapSource source, uint width, uint height)
            {
                IWICFormatConverter converter = WIC.CreateFormatConverter();
                converter.Initialize(source, PixelFormat.Format32bppBGRA, BitmapDitherType.None, null, 0, BitmapPaletteType.Custom);

                if (source.Size.Width == width && source.Size.Height == height) return converter;

                IWICBitmapScaler scaler = WIC.CreateBitmapScaler();
                scaler.Initialize(converter, width, height, BitmapInterpolationMode.HighQualityCubic);

                return scaler;
            }


            uint stride = width * 4;
            uint bufferSize = stride * height;

            byte* pB = (byte*)NativeMemory.Alloc(bufferSize);
            byte* pG = (byte*)NativeMemory.Alloc(bufferSize);
            byte* pR = (byte*)NativeMemory.Alloc(bufferSize);
            byte* pResult = (byte*)NativeMemory.Alloc(bufferSize);

            try
            {
                bResized.CopyPixels(stride, bufferSize, (nint)pB);
                gResized.CopyPixels(stride, bufferSize, (nint)pG);
                rResized.CopyPixels(stride, bufferSize, (nint)pR);

                {
                    byte* pb = pB;
                    byte* pg = pG;
                    byte* pr = pR;
                    byte* pd = pResult;

                    uint pixelCount = width * height;
                    for (int i = 0; i < pixelCount; i++)
                    {
                        pd[0] = pb[0];
                        pd[1] = pg[1];
                        pd[2] = pr[2];
                        pd[3] = 255;

                        pb += 4;
                        pg += 4;
                        pr += 4;
                        pd += 4;
                    }
                }

                ID3D11ShaderResourceView view = CreateShaderResourceView((nint)pResult, width, height, stride, bufferSize, isLinear);
                return view;
            }
            finally
            {
                NativeMemory.Free(pB);
                NativeMemory.Free(pG);
                NativeMemory.Free(pR);
                NativeMemory.Free(pResult);
            }
        }

        private ID3D11ShaderResourceView CreateShaderResourceView(nint data, uint width, uint height, uint stride, uint bufferSize, bool isLinear)
        {
            Texture2DDescription desc = new()
            {
                Width = width,
                Height = height,
                MipLevels = 0,
                ArraySize = 1,
                Format = isLinear ? Format.B8G8R8A8_UNorm : Format.B8G8R8A8_UNorm_SRgb,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.GenerateMips,
            };

            using ID3D11Texture2D texture = Context.Device.CreateTexture2D(desc);
            Context.UpdateSubresource(texture, 0, null, data, stride, bufferSize);

            ID3D11ShaderResourceView view = Context.Device.CreateShaderResourceView(texture);
            Context.GenerateMips(view);

            return view;
        }
    }
}
