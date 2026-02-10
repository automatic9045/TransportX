using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Rendering.Importing
{
    internal readonly struct Texture
    {
        public required readonly string Key { get; init; }
        public required readonly ReadOnlyMemory<byte> Data { get; init; }

        public required readonly int Width { get; init; }
        public required readonly int Height { get; init; }
        public readonly bool IsCompressed => Width == 0 || Height == 0;

        public required readonly string FormatHint { get; init; }
    }
}
