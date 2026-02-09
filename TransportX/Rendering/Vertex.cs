using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        internal static readonly int Size = Marshal.SizeOf<Vertex>();


        public Vector3 Position;
        public Vector3 Normal;
        public Vector4 Color;
        public Vector2 TextureCoord;

        public Vertex(Vector3 position, Vector3 normal, Vector4 color, Vector2 textureCoord)
        {
            Position = position;
            Normal = normal;
            Color = color;
            TextureCoord = textureCoord;
        }

        public Vertex(Vector3 position, Vector4 color) : this(position, Vector3.UnitY, color, Vector2.Zero)
        {
        }
    }
}
