using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace TransportX.Rendering
{
    public class Mesh : IMesh
    {
        public ID3D11Buffer VertexBuffer { get; }
        public ID3D11Buffer IndexBuffer { get; }
        public Material Material { get; }
        public PrimitiveTopology Topology { get; }

        public string? DebugName
        {
            get => field;
            set
            {
                field = value;

                if (value is null)
                {
                    VertexBuffer.DebugName = IndexBuffer.DebugName = null;
                }
                else
                {
                    VertexBuffer.DebugName = $"{value}_VertexBuffer";
                    IndexBuffer.DebugName = $"{value}_IndexBuffer";
                }
            }
        } = null;

        public Mesh(ID3D11Buffer vertexBuffer, ID3D11Buffer indexBuffer, Material material, PrimitiveTopology topology = PrimitiveTopology.TriangleList)
        {
            VertexBuffer = vertexBuffer;
            IndexBuffer = indexBuffer;
            Material = material;
            Topology = topology;
        }

        public static Mesh Create(ID3D11Device device, Vertex[] vertices, int[] indices, Material material,
            PrimitiveTopology topology = PrimitiveTopology.TriangleList)
        {
            BufferDescription vertexBufferDesc = new BufferDescription()
            {
                Usage = ResourceUsage.Immutable,
                ByteWidth = (uint)(Vertex.Size * vertices.Length),
                BindFlags = BindFlags.VertexBuffer,
                CPUAccessFlags = 0,
                MiscFlags = 0,
            };

            GCHandle verticesFixed = GCHandle.Alloc(vertices, GCHandleType.Pinned);
            nint pVertices = verticesFixed.AddrOfPinnedObject();

            SubresourceData vertexBufferData = new SubresourceData(pVertices);
            ID3D11Buffer vertexBuffer = device.CreateBuffer(vertexBufferDesc, vertexBufferData);

            BufferDescription indexBufferDesc = new BufferDescription()
            {
                Usage = ResourceUsage.Immutable,
                ByteWidth = (uint)(sizeof(uint) * indices.Length),
                BindFlags = BindFlags.IndexBuffer,
                CPUAccessFlags = 0,
                MiscFlags = 0,
            };

            GCHandle indicesFixed = GCHandle.Alloc(indices, GCHandleType.Pinned);
            nint pIndices = indicesFixed.AddrOfPinnedObject();

            SubresourceData indexBufferData = new SubresourceData(pIndices);
            ID3D11Buffer indexBuffer = device.CreateBuffer(indexBufferDesc, indexBufferData);

            Mesh mesh = new(vertexBuffer, indexBuffer, material, topology);
            verticesFixed.Free();
            indicesFixed.Free();

            return mesh;
        }

        public void Dispose()
        {
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
        }

        public void Draw(DrawContext context)
        {
            context.DeviceContext.IASetVertexBuffer(0, VertexBuffer, (uint)Vertex.Size, 0);
            context.DeviceContext.IASetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
            context.DeviceContext.IASetPrimitiveTopology(Topology);

            PixelConstantBuffer pixelBuffer = new()
            {
                BaseColor = Material.BaseColor,
                HasTexture = Material.Textures.Count,
            };
            context.DeviceContext.UpdateSubresource(pixelBuffer, context.PixelConstantBuffer);

            context.DeviceContext.PSSetShaderResource(0, 0 < Material.Textures.Count ? Material.Textures[0] : null!);

            context.DeviceContext.DrawIndexed(IndexBuffer.Description.ByteWidth / sizeof(uint), 0, 0);
        }
    }
}
