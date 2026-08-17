using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;
using Vortice.DXGI;

namespace TransportX.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        private static readonly InputElementDescription[] InputElementsKey = [
            new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
            new InputElementDescription("COLOR", 0, Format.R32G32B32A32_Float, InputElementDescription.AppendAligned, 0, InputClassification.PerVertexData, 0),
            new InputElementDescription("NORMAL", 0, Format.R32G32B32_Float, InputElementDescription.AppendAligned, 0, InputClassification.PerVertexData, 0),
            new InputElementDescription("TANGENT", 0, Format.R32G32B32_Float, InputElementDescription.AppendAligned, 0, InputClassification.PerVertexData, 0),
            new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, InputElementDescription.AppendAligned, 0, InputClassification.PerVertexData, 0),
        ];
        public static ReadOnlySpan<InputElementDescription> InputElements => InputElementsKey;

        public static readonly int Size = Marshal.SizeOf<Vertex>();


        public Vector3 Position;
        public Vector4 Color;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector2 TextureCoord;
        public float Padding;

        public Vertex(Vector3 position, Vector3 normal, Vector3 tangent, Vector4 color, Vector2 textureCoord)
        {
            Position = position;
            Normal = normal;
            Tangent = tangent;
            Color = color;
            TextureCoord = textureCoord;
        }

        public Vertex(Vector3 position, Vector4 color) : this(position, Vector3.UnitY, Vector3.UnitX, color, Vector2.Zero)
        {
        }
    }
}
