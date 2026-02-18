using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using DirectXTexNet;
using Vortice.Direct3D11;

namespace TransportX.Rendering
{
    public static class DDSTextureLoader
    {
        private static ID3D11ShaderResourceView Create(ID3D11Device device, ScratchImage image)
        {
            nint pTexture = image.CreateTexture(device.NativePointer);
            if (pTexture == nint.Zero)
            {
                throw new Exception("テクスチャの作成に失敗しました。サポートされないフォーマットかハードウェア機能レベルである可能性があります。");
            }

            using ID3D11Resource resource = new(pTexture);
            return device.CreateShaderResourceView(resource);
        }

        public static ID3D11ShaderResourceView CreateFromFile(ID3D11Device device, string filePath)
        {
            using ScratchImage image = TexHelper.Instance.LoadFromDDSFile(filePath, DDS_FLAGS.NONE);
            return Create(device, image);
        }

        public static ID3D11ShaderResourceView CreateFromMemory(ID3D11Device device, Stream stream)
        {
            long length = stream.Length - stream.Position;
            unsafe
            {
                void* buffer = NativeMemory.Alloc((nuint)length);
                try
                {
                    Span<byte> span = new(buffer, (int)length);
                    stream.ReadExactly(span);

                    using ScratchImage image = TexHelper.Instance.LoadFromDDSMemory((IntPtr)buffer, (nint)length, DDS_FLAGS.NONE);
                    return Create(device, image);
                }
                finally
                {
                    NativeMemory.Free(buffer);
                }
            }
        }
    }
}
