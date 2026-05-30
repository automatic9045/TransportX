using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;
using Vortice.Mathematics;

namespace TransportX.Rendering.Backend
{
    public class RenderTextureArray : IDisposable
    {
        public SizeI Size { get; }
        public ID3D11Texture2D Texture { get; }
        public int Length { get; }
        public IReadOnlyList<ID3D11RenderTargetView> RenderTargetViews { get; }
        public ID3D11ShaderResourceView ShaderResourceView { get; }

        public RenderTextureArray(ID3D11Device device, Texture2DDescription description, int length)
        {
            Size = new SizeI((int)description.Width, (int)description.Height);
            Texture = device.CreateTexture2D(description);

            Length = length;

            ID3D11RenderTargetView[] renderTargetViews = new ID3D11RenderTargetView[Length];
            for (int i = 0; i < Length; i++)
            {
                RenderTargetViewDescription rtvDesc = new RenderTargetViewDescription
                {
                    Format = description.Format,
                    ViewDimension = RenderTargetViewDimension.Texture2DArray,
                    Texture2DArray = new()
                    {
                        FirstArraySlice = (uint)i,
                        ArraySize = 1,
                        MipSlice = 0,
                    },
                };
                renderTargetViews[i] = device.CreateRenderTargetView(Texture, rtvDesc);
            }
            RenderTargetViews = renderTargetViews;

            ShaderResourceView = device.CreateShaderResourceView(Texture);
        }

        public void Dispose()
        {
            ShaderResourceView.Dispose();
            for (int i = 0; i < Length; i++)
            {
                RenderTargetViews[i].Dispose();
            }
            Texture.Dispose();
        }
    }
}
