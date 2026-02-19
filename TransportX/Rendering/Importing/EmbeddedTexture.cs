using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Rendering.Importing
{
    internal readonly struct EmbeddedTexture
    {
        public required readonly string Key { get; init; }
        public required readonly ReadOnlyMemory<byte> Data { get; init; }
        public required readonly TextureFormat Format { get; init; }
        public required readonly int Width { get; init; }
        public required readonly int Height { get; init; }
    }
}
