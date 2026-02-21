using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.Mathematics;

using TransportX.Rendering;

namespace TransportX.Extensions.Rendering
{
    public class DynamicLineMesh : IMesh
    {
        private readonly ID3D11Buffer VertexBuffer;

        public BoundingBox BoundingBox { get; private set; } = new(Vector3.Zero, Vector3.Zero);
        public Material Material { get; }

        public string? DebugName
        {
            get => field;
            set
            {
                field = value;
                VertexBuffer.DebugName = value is null ? null : $"{value}_VertexBuffer";
            }
        } = null;

        public DynamicLineMesh(ID3D11Device device, Material material)
        {
            BufferDescription desc = new()
            {
                ByteWidth = (uint)(Unsafe.SizeOf<Vertex>() * 2),
                Usage = ResourceUsage.Dynamic,
                BindFlags = BindFlags.VertexBuffer,
                CPUAccessFlags = CpuAccessFlags.Write,
                MiscFlags = ResourceOptionFlags.None,
            };
            VertexBuffer = device.CreateBuffer(new Vertex[2], desc);

            Material = material;
        }

        public void Dispose()
        {
            VertexBuffer.Dispose();
        }

        public void SetVector(ID3D11DeviceContext context, Vector3 vector)
        {
            Span<Vertex> vertices = [
                new Vertex(Vector3.Zero, Vector4.One),
                new Vertex(vector, Vector4.One),
            ];

            MappedSubresource mapped = context.Map(VertexBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
            unsafe
            {
                Span<Vertex> dest = new(mapped.DataPointer.ToPointer(), 2);
                vertices.CopyTo(dest);
            }
            context.Unmap(VertexBuffer, 0);

            BoundingBox = BoundingBox.CreateFromPoints([Vector3.Zero, vector]);
        }

        public void Draw(DrawContext context)
        {
            context.DeviceContext.IASetVertexBuffer(0, VertexBuffer, (uint)Unsafe.SizeOf<Vertex>(), 0);
            context.DeviceContext.IASetPrimitiveTopology(PrimitiveTopology.LineList);

            MaterialConstants materialConstants = new()
            {
                BaseColor = Material.BaseColor,
            };
            context.DeviceContext.UpdateSubresource(materialConstants, context.MaterialBuffer);

            context.DeviceContext.PSSetShaderResource(0, null!);

            context.DeviceContext.Draw(2, 0);
        }
    }
}
