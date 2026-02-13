using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Rendering.Importing
{
    internal readonly struct Material
    {
        public required readonly string Name { get; init; }

        public required readonly Vector4 BaseColor { get; init; }
        public required readonly float Metallic { get; init; }
        public required readonly float Roughness { get; init; }
        public required readonly Vector3 Emissive { get; init; }

        public required readonly TextureReference? BaseColorTexture { get; init; }
        public required readonly TextureReference? NormalTexture { get; init; }
        public required readonly TextureReference? OcclusionTexture { get; init; }
        public required readonly TextureReference? RoughnessTexture { get; init; }
        public required readonly TextureReference? MetallicTexture { get; init; }
        public required readonly TextureReference? EmissiveTexture { get; init; }
    }
}
