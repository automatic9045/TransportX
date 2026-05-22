using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace TransportX.Rendering.Shadows
{
    public class ShadowMap : IDisposable
    {
        private readonly ID3D11Texture2D ShadowTexture;

        public int Resolution { get; }
        public int CascadeCount { get; }

        public IReadOnlyList<ID3D11DepthStencilView> DepthStencilViews { get; }
        public ID3D11ShaderResourceView ShaderResourceView { get; }

        public ShadowMap(ID3D11Device device, int resolution, int cascadeCount)
        {
            Resolution = resolution;
            CascadeCount = cascadeCount;

            if (resolution <= 0)
            {
                Texture2DDescription desc = new()
                {
                    Width = 1,
                    Height = 1,
                    MipLevels = 1,
                    ArraySize = (uint)CascadeCount,
                    Format = Format.R32_Float, // R32_Floatで直接データを流し込む
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Immutable,
                    BindFlags = BindFlags.ShaderResource, // DSVは不要
                    CPUAccessFlags = CpuAccessFlags.None,
                    MiscFlags = ResourceOptionFlags.None,
                };

                unsafe
                {
                    float[] dummyData = new float[CascadeCount];
                    Array.Fill(dummyData, 1.0f);

                    fixed (float* p = dummyData)
                    {
                        SubresourceData[] initData = new SubresourceData[CascadeCount];
                        for (int i = 0; i < CascadeCount; i++)
                        {
                            initData[i] = new SubresourceData((IntPtr)(p + i), 4, 4);
                        }
                        ShadowTexture = device.CreateTexture2D(desc, initData);
                    }
                }

                DepthStencilViews = [];
            }
            else
            {
                Texture2DDescription desc = new()
                {
                    Width = (uint)Resolution,
                    Height = (uint)Resolution,
                    MipLevels = 1,
                    ArraySize = (uint)CascadeCount,
                    Format = Format.R32_Typeless,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                    CPUAccessFlags = CpuAccessFlags.None,
                    MiscFlags = ResourceOptionFlags.None,
                };
                ShadowTexture = device.CreateTexture2D(desc);

                ID3D11DepthStencilView[] depthStencilViews = new ID3D11DepthStencilView[CascadeCount];
                for (int i = 0; i < CascadeCount; i++)
                {
                    DepthStencilViewDescription dsvDesc = new()
                    {
                        Format = Format.D32_Float,
                        ViewDimension = DepthStencilViewDimension.Texture2DArray,
                        Texture2DArray = new Texture2DArrayDepthStencilView()
                        {
                            MipSlice = 0,
                            FirstArraySlice = (uint)i,
                            ArraySize = 1,
                        }
                    };
                    depthStencilViews[i] = device.CreateDepthStencilView(ShadowTexture, dsvDesc);
                }
                DepthStencilViews = depthStencilViews;
            }

            ShaderResourceViewDescription srvDesc = new()
            {
                Format = Format.R32_Float,
                ViewDimension = ShaderResourceViewDimension.Texture2DArray,
                Texture2DArray = new Texture2DArrayShaderResourceView()
                {
                    MipLevels = 1,
                    FirstArraySlice = 0,
                    ArraySize = (uint)CascadeCount,
                },
            };
            ShaderResourceView = device.CreateShaderResourceView(ShadowTexture, srvDesc);
        }

        public void Dispose()
        {
            ShaderResourceView.Dispose();
            for (int i = 0; i < DepthStencilViews.Count; i++)
            {
                DepthStencilViews[i].Dispose();
            }
            ShadowTexture.Dispose();
        }
    }
}
