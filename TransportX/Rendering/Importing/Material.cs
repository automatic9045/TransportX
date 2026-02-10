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
        public required readonly TextureReference[] Textures { get; init; }
    }
}
