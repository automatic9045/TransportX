using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DirectXTexNet;
using Vortice.Direct3D11;

namespace TransportX.Rendering
{
    public class DDSTextureFactory
    {
        private readonly ID3D11Device Device;

        public DDSTextureFactory(ID3D11Device device)
        {
            Device = device;
        }

        private ID3D11ShaderResourceView Create(ScratchImage image)
        {
            nint pTexture = image.CreateTexture(Device.NativePointer);
            if (pTexture == nint.Zero)
            {
                throw new Exception("テクスチャの作成に失敗しました。サポートされないフォーマットかハードウェア機能レベルである可能性があります。");
            }

            using ID3D11Resource resource = new(pTexture);
            return Device.CreateShaderResourceView(resource);
        }

        public ID3D11ShaderResourceView CreateFromFile(string filePath)
        {
            using ScratchImage image = TexHelper.Instance.LoadFromDDSFile(filePath, DDS_FLAGS.NONE);
            return Create(image);
        }

        public unsafe ID3D11ShaderResourceView CreateFromMemory(ReadOnlySpan<byte> data)
        {
            fixed (byte* pData = data)
            {
                using ScratchImage image = TexHelper.Instance.LoadFromDDSMemory((IntPtr)pData, data.Length, DDS_FLAGS.NONE);
                return Create(image);
            }
        }
    }
}
