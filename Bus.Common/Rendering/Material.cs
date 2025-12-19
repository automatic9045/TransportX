using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

namespace Bus.Common.Rendering
{
    public class Material
    {
        public static readonly Material Default = new(Vector4.One, []);


        public Vector4 BaseColor { get; set; }
        public List<ID3D11ShaderResourceView> Textures { get; }

        public Material(Vector4 baseColor, List<ID3D11ShaderResourceView> textures)
        {
            BaseColor = baseColor;
            Textures = textures;
        }
    }
}
