using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

namespace TransportX.Rendering
{
    public class Material : ICloneable
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

        public ID3D11ShaderResourceView?[] TextureViews
        {
            get
            {
                field[0] = BaseColorTexture;
                field[1] = NormalTexture;
                field[2] = ORMTexture;
                field[3] = EmissiveTexture;
                return field;
            }
        } = new ID3D11ShaderResourceView?[4];

        public string? DebugName
        {
            get => field;
            set
            {
                field = value;

                if (value is null)
                {
                    BaseColorTexture?.DebugName = NormalTexture?.DebugName = ORMTexture?.DebugName = EmissiveTexture?.DebugName = null;
                }
                else
                {
                    BaseColorTexture?.DebugName = $"{value}_BaseColorTexture";
                    NormalTexture?.DebugName = $"{value}_NormalTexture";
                    ORMTexture?.DebugName = $"{value}_ORMTexture";
                    EmissiveTexture?.DebugName = $"{value}_EmissiveTexture";
                }
            }
        } = null;

        public Material()
        {
        }

        public Material Clone() => new()
        {
            BaseColor = BaseColor,
            Metallic = Metallic,
            Roughness = Roughness,
            Emissive = Emissive,

            BaseColorTexture = BaseColorTexture,
            NormalTexture = NormalTexture,
            ORMTexture = ORMTexture,
            EmissiveTexture = EmissiveTexture,

            DebugName = DebugName,
        };

        object ICloneable.Clone() => Clone();
    }
}
