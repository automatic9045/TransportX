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
    public struct TransformBuffer
    {
        internal static readonly int Size = Marshal.SizeOf<TransformBuffer>();


        public Matrix4x4 World;
        public Matrix4x4 View;
        public Matrix4x4 Projection;

        public TransformBuffer()
        {
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct MaterialBuffer
    {
        internal static readonly int Size = Marshal.SizeOf<MaterialBuffer>();


        public Vector4 BaseColor = Vector4.One;
        public Vector3 Emissive = Vector3.Zero;
        public float Roughness = 1;
        public float Metallic = 0;
        public int HasBaseTexture = 0;
        public int HasNormalTexture = 0;
        public int HasORMTexture = 0;
        public int HasEmissiveTexture = 0;
        public Vector3 Padding;

        public MaterialBuffer()
        {
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct SceneBuffer
    {
        internal static readonly int Size = Marshal.SizeOf<SceneBuffer>();


        public Vector3 ToLight = Vector3.Zero;
        public float Padding1;
        public Vector3 CameraPosition = Vector3.Zero;
        public float Padding2;

        public SceneBuffer()
        {
        }
    }
}
