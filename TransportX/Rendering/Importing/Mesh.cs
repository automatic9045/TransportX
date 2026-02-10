using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Rendering.Importing
{
    internal readonly struct Mesh
    {
        public required readonly string Name { get; init; }
        public required readonly Vector3[] Vertices { get; init; }
        public required readonly int[] Indices { get; init; }
        public required readonly Vector3[] Normals { get; init; }
        public required readonly Vector4[]? Colors { get; init; }
        public required readonly Vector2[]? TextureCoords { get; init; }
        public required readonly int MaterialIndex { get; init; }
    }
}
