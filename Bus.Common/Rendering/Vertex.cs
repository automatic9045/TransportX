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


        public float X, Y, Z;
        public Vector2 TextureCoord;
        //public Vector4 Color;
    }
}
