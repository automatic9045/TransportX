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

using TransportX.Rendering.Backend;

namespace TransportX.Rendering
{
    public class Mesh : IMesh
    {
        private readonly ID3D11Buffer[] SetVertexBuffersArray;

        public string Name { get; }
        public ID3D11Buffer VertexBuffer { get; }
        public ID3D11Buffer IndexBuffer { get; }
        public PrimitiveTopology Topology { get; }
        public BoundingBox BoundingBox { get; }
        public Material Material { get; }

        private string? DebugNameKey;
        public string? DebugName
        {
            get => DebugNameKey;
            set
            {
                DebugNameKey = value;

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
        }

        public Mesh(string name, ID3D11Buffer vertexBuffer, ID3D11Buffer indexBuffer, BoundingBox boundingBox, Material material,
            PrimitiveTopology topology = PrimitiveTopology.TriangleList)
        {
            Name = DebugNameKey = name;
            VertexBuffer = vertexBuffer;
            IndexBuffer = indexBuffer;
            Topology = topology;
            BoundingBox = boundingBox;
            Material = material;

            SetVertexBuffersArray = new ID3D11Buffer[2];
            SetVertexBuffersArray[0] = VertexBuffer;
        }

        public static unsafe Mesh Create(ID3D11Device device, string name, Vertex[] vertices, int[] indices, Material material,
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

                Mesh mesh = new(name, vertexBuffer, indexBuffer, boundingBox, material, topology);
                return mesh;
            }
        }

        public void Dispose()
        {
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
        }

        public void Draw(in DrawContext context, Material renderMaterial)
        {
            SetVertexBuffersArray[1] = context.InstanceBuffer;
            context.DeviceContext.IASetVertexBuffers(0, 2, SetVertexBuffersArray, [(uint)Vertex.Size, (uint)InstanceData.Size], [0, 0]);

            context.DeviceContext.IASetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
            context.DeviceContext.IASetPrimitiveTopology(Topology);

            MaterialConstants materialConstants = new()
            {
                BaseColor = renderMaterial.BaseColor,
                Emissive = renderMaterial.Emissive * 0.001f,
                Roughness = renderMaterial.Roughness,
                Metallic = renderMaterial.Metallic,

                BaseTextureMode = (uint)renderMaterial.BaseColorTexture.Mode,
                NormalTextureMode = (uint)renderMaterial.NormalTexture.Mode,
                ORMTextureMode = (uint)renderMaterial.ORMTexture.Mode,
                EmissiveTextureMode = (uint)renderMaterial.EmissiveTexture.Mode,
            };
            context.DeviceContext.UpdateSubresource(materialConstants, context.MaterialBuffer);

            context.DeviceContext.PSSetShaderResources(0, renderMaterial.TextureViews!);

            uint indexCount = IndexBuffer.Description.ByteWidth / sizeof(uint);
            context.DeviceContext.DrawIndexedInstanced(indexCount, (uint)context.InstanceCount, 0, 0, 0);
        }

        public void Draw(in DrawContext context)
        {
            Draw(context, Material);
        }
    }
}
