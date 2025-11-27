using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        internal static readonly int Size = Marshal.SizeOf<Vertex>();


        public Vector3 Position;
        public Vector3 Normal;
        public Vector4 Color;
        public Vector2 TextureCoord;
    }
}
