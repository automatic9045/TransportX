using System;
using System.Buffers;
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
        private static readonly IReadOnlyDictionary<string, EmbeddedTexture> EmptyEmbeddedTextureMap = new Dictionary<string, EmbeddedTexture>();

        internal RawTextureFactory RawFactory { get; }
        internal WICTextureFactory WICFactory { get; }
        internal DDSTextureFactory DDSFactory { get; }

        public TextureFactory(ID3D11DeviceContext context, IWICImagingFactory wic)
        {
            RawFactory = new RawTextureFactory(context);
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

        internal ID3D11ShaderResourceView CreateFromMemory(EmbeddedTexture embeddedTexture, bool isLinear, IErrorCollector errorCollector)
        {
            switch (embeddedTexture.Format)
            {
                case TextureFormat.Uncompressed:
                    unsafe
                    {
                        RawTextureFactory.ReportIfNpot(embeddedTexture.Width, embeddedTexture.Height, errorCollector);

                        using MemoryHandle handle = embeddedTexture.Data.Pin();
                        RawTextureFactory.Description desc = new()
                        {
                            Width = (uint)embeddedTexture.Width,
                            Height = (uint)embeddedTexture.Height,
                            IsLinear = isLinear,
                            SrcData = (nint)handle.Pointer,
                            SrcRowPitch = (uint)embeddedTexture.Width * 4,
                            SrcDepthPitch = 0,
                        };

                        return RawFactory.Create(desc);
                    }

                case TextureFormat.WIC:
                    return WICFactory.CreateFromMemory(embeddedTexture.Data.Span, isLinear, errorCollector);

                case TextureFormat.DDS:
                    return DDSFactory.CreateFromMemory(embeddedTexture.Data.Span);

                default:
                    throw new NotSupportedException();
            }
        }

        internal ID3D11ShaderResourceView? CreateMergedFromReferences(TextureReference? rRef, TextureReference? gRef, TextureReference? bRef,
            string baseDirectory, IReadOnlyDictionary<string, EmbeddedTexture> embeddedTextures, bool isLinear, IErrorCollector errorCollector)
        {
            if (!rRef.HasValue && !gRef.HasValue && !bRef.HasValue) return null;

            IWICStream? rStream = null;
            IWICStream? gStream = null;
            IWICStream? bStream = null;

            try
            {
                if (rRef.HasValue) rStream = OpenReferenceStream(rRef.Value);
                if (gRef.HasValue) gStream = OpenReferenceStream(gRef.Value);
                if (bRef.HasValue) bStream = OpenReferenceStream(bRef.Value);

                if (rStream is null && gStream is null && bStream is null)
                {
                    return null;
                }

                return WICFactory.CreateFromMerged(rStream, gStream, bStream, isLinear, errorCollector);
            }
            finally
            {
                rStream?.Dispose();
                gStream?.Dispose();
                bStream?.Dispose();
            }


            IWICStream? OpenReferenceStream(TextureReference textureRef)
            {
                if (textureRef.Type == TextureReference.TextureType.File)
                {
                    string path = Path.Combine(baseDirectory, textureRef.Key);

                    string extension = Path.GetExtension(textureRef.Key).ToLowerInvariant();
                    switch (extension)
                    {
                        case ".dds":
                        case ".tga":
                        case ".ktx":
                        case ".ktx2":
                            Error error = new(ErrorLevel.Error, $"{extension} 形式のテクスチャ '{path}' は 3 テクスチャの合成に使用できません。" +
                                "テクスチャの合成は WIC 形式のみ対応しています。", null);
                            errorCollector.Report(error);
                            return null;
                    }

                    return WICFactory.CreateStreamFromFile(path);
                }
                else if (textureRef.Type == TextureReference.TextureType.Embedded)
                {
                    if (embeddedTextures.TryGetValue(textureRef.Key, out EmbeddedTexture embeddedTexture))
                    {
                        if (embeddedTexture.Format != TextureFormat.WIC)
                        {
                            Error error = new(ErrorLevel.Error, $"{embeddedTexture.Format} 形式のテクスチャ '{textureRef.Key}' は 3 テクスチャの合成に使用できません。" +
                                "テクスチャの合成は WIC 形式のみ対応しています。", null);
                            errorCollector.Report(error);
                            return null;
                        }

                        return WICFactory.CreateStreamFromMemory(embeddedTexture.Data.Span);
                    }
                }

                return null;
            }
        }

        public ID3D11ShaderResourceView? CreateMergedFromFiles(string? rFilePath, string? gFilePath, string? bFilePath, bool isLinear, IErrorCollector errorCollector)
        {
            if (rFilePath is null && gFilePath is null && bFilePath is null) return null;
            if (rFilePath is not null && rFilePath == gFilePath && gFilePath == bFilePath) return CreateFromFile(bFilePath, isLinear, errorCollector);

            TextureReference? rRef = rFilePath is null ? null : new(TextureReference.TextureType.File, rFilePath);
            TextureReference? gRef = gFilePath is null ? null : new(TextureReference.TextureType.File, gFilePath);
            TextureReference? bRef = bFilePath is null ? null : new(TextureReference.TextureType.File, bFilePath);

            return CreateMergedFromReferences(rRef, gRef, bRef, string.Empty, EmptyEmbeddedTextureMap, isLinear, errorCollector);
        }
    }
}
