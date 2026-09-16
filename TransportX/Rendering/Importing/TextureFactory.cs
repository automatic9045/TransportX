using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;
using Vortice.DXGI;

using TransportX.Diagnostics;

namespace TransportX.Rendering.Importing
{
    internal static class TextureFactory
    {
        public static void ReportIfNpot(int width, int height, IErrorCollector errorCollector)
        {
            if (!IsPowerOfTwo(width) || !IsPowerOfTwo(height))
            {
                Error error = new(ErrorLevel.Warning, $"テクスチャサイズ ({width}x{height}) が 2 の累乗ではありません。パフォーマンスが低下する可能性があります。", null);
                errorCollector.Report(error);
            }


            static bool IsPowerOfTwo(int x) => 0 < x && (x & (x - 1)) == 0;
        }

        public static ID3D11ShaderResourceView Create(ID3D11DeviceContext context, in Description desc)
        {
            Texture2DDescription tempTextureDesc = new()
            {
                Width = desc.Width,
                Height = desc.Height,
                MipLevels = 0,
                ArraySize = 1,
                Format = desc.IsLinear ? Format.B8G8R8A8_UNorm : Format.B8G8R8A8_UNorm_SRgb,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.GenerateMips,
            };

            using ID3D11Texture2D tempTexture = context.Device.CreateTexture2D(tempTextureDesc);
            context.UpdateSubresource(tempTexture, 0, null, desc.SrcData, desc.SrcRowPitch, desc.SrcDepthPitch);

            using ID3D11ShaderResourceView tempView = context.Device.CreateShaderResourceView(tempTexture);
            context.GenerateMips(tempView);

            Texture2DDescription textureDesc = tempTextureDesc with
            {
                MipLevels = tempTexture.Description.MipLevels,
                BindFlags = BindFlags.ShaderResource,
                MiscFlags = ResourceOptionFlags.None,
            };

            using ID3D11Texture2D texture = context.Device.CreateTexture2D(textureDesc);
            context.CopyResource(texture, tempTexture);

            ID3D11ShaderResourceView view = context.Device.CreateShaderResourceView(texture);
            return view;
        }

        public readonly struct Description
        {
            public required uint Width { get; init; }
            public required uint Height { get; init; }
            public required bool IsLinear { get; init; }
            public required nint SrcData { get; init; }
            public required uint SrcRowPitch { get; init; }
            public required uint SrcDepthPitch { get; init; }
        }
    }
}
