using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

namespace TransportX.Rendering
{
    public class Material
    {
        public static Material Default() => new()
        {
            BaseColor = Vector4.One,
            Metallic = 0,
            Roughness = 1,
            Emissive = Vector3.Zero,
        };


        public required Vector4 BaseColor { get; set; }
        public required float Metallic { get; set; }
        public required float Roughness { get; set; }
        public required Vector3 Emissive { get; set; }

        public ID3D11ShaderResourceView? BaseColorTexture { get; set; } = null;
        public ID3D11ShaderResourceView? NormalTexture { get; set; } = null;
        public ID3D11ShaderResourceView? ORMTexture { get; set; } = null;
        public ID3D11ShaderResourceView? EmissiveTexture { get; set; } = null;

        public Material()
        {
        }
    }
}
