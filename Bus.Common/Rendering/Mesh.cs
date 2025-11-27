using Assimp;
using SharpGen.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Bus.Common.Rendering
{
    public class Mesh : IDisposable
    {
        private readonly ID3D11Buffer VertexBuffer;
        private readonly ID3D11Buffer IndexBuffer;
        private readonly IReadOnlyList<ID3D11ShaderResourceView> Textures;

        private event EventHandler? Disposing;

        public Mesh(ID3D11Buffer vertexBuffer, ID3D11Buffer indexBuffer, IReadOnlyList<ID3D11ShaderResourceView> textures)
        {
            VertexBuffer = vertexBuffer;
            IndexBuffer = indexBuffer;
            Textures = textures;
        }

        public static Mesh Create(ID3D11Device device, Vertex[] vertices, int[] indices, IReadOnlyList<ID3D11ShaderResourceView> textures)
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

            Mesh mesh = new Mesh(vertexBuffer, indexBuffer, textures);
            mesh.Disposing += (sender, e) =>
            {
                verticesFixed.Free();
                indicesFixed.Free();
            };

            return mesh;
        }

        public void Dispose()
        {
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();

            Disposing?.Invoke(this, EventArgs.Empty);
        }

        public void Draw(DrawContext context)
        {
            context.DeviceContext.IASetVertexBuffer(0, VertexBuffer, (uint)Vertex.Size, 0);
            context.DeviceContext.IASetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);

            PixelConstantBuffer pixelBuffer = new()
            {
                HasTexture = Textures.Count,
            };
            context.DeviceContext.UpdateSubresource(pixelBuffer, context.PixelConstantBuffer);

            context.DeviceContext.PSSetShaderResource(0, 0 < Textures.Count ? Textures[0] : null!);

            context.DeviceContext.DrawIndexed(IndexBuffer.Description.ByteWidth / sizeof(uint), 0, 0);
        }
    }
}
