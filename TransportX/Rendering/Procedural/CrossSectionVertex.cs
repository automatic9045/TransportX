using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Rendering.Procedural
{
    public readonly struct CrossSectionVertex
    {
        public required Vector2 Position { get; init; }
        public required Vector2 Normal { get; init; }
        public required float U { get; init; }
        public Vector4 Color { get; init; } = Vector4.One;

        [SetsRequiredMembers]
        public CrossSectionVertex(Vector2 position, Vector2 normal, float u)
        {
            Position = position;
            Normal = normal;
            U = u;
        }
    }
}
