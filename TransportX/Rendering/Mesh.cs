using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace TransportX.Rendering
{
    public class Mesh : IMesh
    {
        private readonly ID3D11ShaderResourceView?[] TextureViews;

        public ID3D11Buffer VertexBuffer { get; }
        public ID3D11Buffer IndexBuffer { get; }
        public PrimitiveTopology Topology { get; }
        public BoundingBox BoundingBox { get; }
        public Material Material { get; }

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

        public Mesh(ID3D11Buffer vertexBuffer, ID3D11Buffer indexBuffer, BoundingBox boundingBox, Material material, PrimitiveTopology topology = PrimitiveTopology.TriangleList)
        {
            VertexBuffer = vertexBuffer;
            IndexBuffer = indexBuffer;
            Topology = topology;
            BoundingBox = boundingBox;
            Material = material;

            TextureViews = [
                Material.BaseColorTexture,
                Material.NormalTexture,
                Material.ORMTexture,
                Material.EmissiveTexture
            ];
        }

        public static unsafe Mesh Create(ID3D11Device device, Vertex[] vertices, int[] indices, Material material,
            PrimitiveTopology topology = PrimitiveTopology.TriangleList)
        {
            Vector3 min = new(float.MaxValue);
            Vector3 max = new(float.MinValue);

            foreach (Vertex vertex in vertices)
            {
                min = Vector3.Min(min, vertex.Position);
                max = Vector3.Max(max, vertex.Position);
            }

            BoundingBox boundingBox = new BoundingBox(min, max);

            BufferDescription vertexBufferDesc = new()
            {
                Usage = ResourceUsage.Immutable,
                ByteWidth = (uint)(Vertex.Size * vertices.Length),
                BindFlags = BindFlags.VertexBuffer,
                CPUAccessFlags = 0,
                MiscFlags = 0,
            };

            BufferDescription indexBufferDesc = new()
            {
                Usage = ResourceUsage.Immutable,
                ByteWidth = (uint)(sizeof(uint) * indices.Length),
                BindFlags = BindFlags.IndexBuffer,
                CPUAccessFlags = 0,
                MiscFlags = 0,
            };

            fixed (void* pVertices = vertices)
            fixed (void* pIndices = indices)
            {
                SubresourceData vertexBufferData = new(pVertices);
                ID3D11Buffer vertexBuffer = device.CreateBuffer(vertexBufferDesc, vertexBufferData);

                SubresourceData indexBufferData = new SubresourceData(pIndices);
                ID3D11Buffer indexBuffer = device.CreateBuffer(indexBufferDesc, indexBufferData);

                Mesh mesh = new(vertexBuffer, indexBuffer, boundingBox, material, topology);
                return mesh;
            }
        }

        public void Dispose()
        {
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
        }

        public void Draw(in DrawContext context)
        {
            context.DeviceContext.IASetVertexBuffer(0, VertexBuffer, (uint)Vertex.Size, 0);
            context.DeviceContext.IASetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
            context.DeviceContext.IASetPrimitiveTopology(Topology);

            MaterialConstants materialConstants = new()
            {
                BaseColor = Material.BaseColor,
                Emissive = Material.Emissive,
                Roughness = Material.Roughness,
                Metallic = Material.Metallic,

                HasBaseTexture = BoolToInt32(Material.BaseColorTexture is not null),
                HasNormalTexture = BoolToInt32(Material.NormalTexture is not null),
                HasORMTexture = BoolToInt32(Material.ORMTexture is not null),
                HasEmissiveTexture = BoolToInt32(Material.EmissiveTexture is not null),
            };
            context.DeviceContext.UpdateSubresource(materialConstants, context.MaterialBuffer);

            context.DeviceContext.PSSetShaderResources(0, TextureViews!);

            context.DeviceContext.DrawIndexed(IndexBuffer.Description.ByteWidth / sizeof(uint), 0, 0);


            static int BoolToInt32(bool value) => value ? 1 : 0;
        }
    }
}
