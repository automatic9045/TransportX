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
    public struct VertexConstantBuffer
    {
        internal static readonly int Size = Marshal.SizeOf<VertexConstantBuffer>();


        public Matrix4x4 World;
        public Matrix4x4 View;
        public Matrix4x4 Projection;
        public Vector4 Light;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct PixelConstantBuffer
    {
        internal static readonly int Size = Marshal.SizeOf<PixelConstantBuffer>();


        public Vector4 BaseColor;
        public int HasTexture;
        public Vector3 Padding;
    }
}
