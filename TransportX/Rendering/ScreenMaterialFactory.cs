using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

namespace TransportX.Rendering
{
    public static class ScreenMaterialFactory
    {
        public static Material Create(Material target, ID3D11ShaderResourceView shaderResourceView)
        {
            Material material = target.Clone();

            material.Metallic = 0;
            material.Roughness = 1;
            material.BaseColor = Vector4.UnitW;
            material.BaseColorTexture = TextureSlot.Empty;

            material.Emissive = Vector3.One * 1000;
            material.EmissiveTexture = new TextureSlot(shaderResourceView, TextureSampleMode.Texture);

            return material;
        }
    }
}
