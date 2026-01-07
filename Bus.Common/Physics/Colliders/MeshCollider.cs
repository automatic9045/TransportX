using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics.Collidables;
using ColliderMesh = BepuPhysics.Collidables.Mesh;
using Vortice.Direct3D;
using Vortice.Direct3D11;

using Bus.Common.Rendering;

namespace Bus.Common.Physics.Colliders
{
    public class MeshCollider : Collider<ColliderMesh>
    {
        public bool IsOpen { get; }

        protected readonly Material DebugMaterial = new(Vector4.One, []);
        public override Vector4 DebugModelColor
        {
            get => DebugMaterial.BaseColor;
            set => DebugMaterial.BaseColor = value;
        }

        public MeshCollider(ColliderMesh shape, TypedIndex shapeIndex, ColliderMaterial material, Matrix4x4 offset, bool isOpen)
            : base(shape, shapeIndex, material, offset, (s, m) => isOpen ? s.ComputeOpenInertia(m) : s.ComputeClosedInertia(m))
        {
            IsOpen = isOpen;
        }

        public override void CreateDebugModel(ID3D11Device device)
        {
            if (DebugModel is not null) throw new InvalidOperationException("モデルは既に作成されています。");

            int triangleCount = Shape.Triangles.Length;
            Vertex[] vertices = new Vertex[triangleCount * 3];
            int[] indices = new int[triangleCount * 6];

            for (int i = 0; i < triangleCount; i++)
            {
                Triangle triangle = Shape.Triangles[i];
                int baseIndex = i * 3;

                vertices[baseIndex] = new(triangle.A, Vector4.One);
                vertices[baseIndex + 1] = new(triangle.B, Vector4.One);
                vertices[baseIndex + 2] = new(triangle.C, Vector4.One);

                int indexOffset = i * 6;

                indices[indexOffset] = baseIndex;
                indices[indexOffset + 1] = baseIndex + 1;

                indices[indexOffset + 2] = baseIndex + 1;
                indices[indexOffset + 3] = baseIndex + 2;

                indices[indexOffset + 4] = baseIndex + 2;
                indices[indexOffset + 5] = baseIndex;
            }

            Rendering.Mesh visualMesh = Rendering.Mesh.Create(device, vertices, indices, DebugMaterial, PrimitiveTopology.LineList);
            DebugModel = new Model([visualMesh], []);
            DebugName = DebugName;
        }
    }
}
