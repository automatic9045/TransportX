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

        public TextureSlot BaseColorTexture { get; set; } = TextureSlot.Empty;
        public TextureSlot NormalTexture { get; set; } = TextureSlot.Empty;
        public TextureSlot ORMTexture { get; set; } = TextureSlot.Empty;
        public TextureSlot EmissiveTexture { get; set; } = TextureSlot.Empty;

        public ID3D11ShaderResourceView?[] TextureViews
        {
            get
            {
                field[0] = BaseColorTexture.View;
                field[1] = NormalTexture.View;
                field[2] = ORMTexture.View;
                field[3] = EmissiveTexture.View;
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
                    BaseColorTexture.View?.DebugName = NormalTexture.View?.DebugName = ORMTexture.View?.DebugName = EmissiveTexture.View?.DebugName = null;
                }
                else
                {
                    BaseColorTexture.View?.DebugName = $"{value}_BaseColorTexture";
                    NormalTexture.View?.DebugName = $"{value}_NormalTexture";
                    ORMTexture.View?.DebugName = $"{value}_ORMTexture";
                    EmissiveTexture.View?.DebugName = $"{value}_EmissiveTexture";
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
