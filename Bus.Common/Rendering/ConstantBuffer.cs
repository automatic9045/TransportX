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
    public struct ConstantBuffer
    {
        internal static readonly int Size = Marshal.SizeOf<ConstantBuffer>();


        public Matrix4x4 World;
        public Matrix4x4 View;
        public Matrix4x4 Projection;
        public Vector4 Light;
    }
}
