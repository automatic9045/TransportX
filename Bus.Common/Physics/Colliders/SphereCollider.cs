using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics.Collidables;
using Vortice.Direct3D;
using Vortice.Direct3D11;

using Bus.Common.Rendering;

namespace Bus.Common.Physics.Colliders
{
    public class SphereCollider : Collider<Sphere>
    {
        private const int SegmentCount = 16;


        protected readonly Material DebugMaterial = new(Vector4.One, []);
        public override Vector4 DebugColor
        {
            get => DebugMaterial.BaseColor;
            set => DebugMaterial.BaseColor = value;
        }

        public SphereCollider(Sphere shape, TypedIndex shapeIndex, ColliderMaterial material, Pose offset)
            : base(shape, shapeIndex, material, offset, (s, m) => s.ComputeInertia(m))
        {
        }

        public override void CreateDebugResources(ID3D11Device device)
        {
            if (DebugModel is not null) throw new InvalidOperationException("モデルは既に作成されています。");

            float radius = Shape.Radius;
            float angleStep = float.Pi * 2 / SegmentCount;

            Vertex[] vertices = new Vertex[SegmentCount * 3];
            int[] indices = new int[SegmentCount * 2 * 3];

            int vIndex = 0;
            int iIndex = 0;

            int baseIndexXY = vIndex;
            for (int i = 0; i < SegmentCount; i++)
            {
                float angle = i * angleStep;
                Vector3 p = new(float.Cos(angle) * radius, float.Sin(angle) * radius, 0);
                vertices[vIndex++] = new Vertex(p, Vector4.One);

                indices[iIndex++] = baseIndexXY + i;
                indices[iIndex++] = baseIndexXY + (i + 1) % SegmentCount;
            }

            int baseIndexYZ = vIndex;
            for (int i = 0; i < SegmentCount; i++)
            {
                float angle = i * angleStep;
                Vector3 p = new(0, float.Cos(angle) * radius, float.Sin(angle) * radius);
                vertices[vIndex++] = new Vertex(p, Vector4.One);

                indices[iIndex++] = baseIndexYZ + i;
                indices[iIndex++] = baseIndexYZ + (i + 1) % SegmentCount;
            }

            int baseIndexXZ = vIndex;
            for (int i = 0; i < SegmentCount; i++)
            {
                float angle = i * angleStep;
                Vector3 p = new(float.Cos(angle) * radius, 0, float.Sin(angle) * radius);
                vertices[vIndex++] = new Vertex(p, Vector4.One);

                indices[iIndex++] = baseIndexXZ + i;
                indices[iIndex++] = baseIndexXZ + (i + 1) % SegmentCount;
            }

            Rendering.Mesh visualMesh = Rendering.Mesh.Create(device, vertices, indices, DebugMaterial, PrimitiveTopology.LineList);
            DebugModel = new Model([visualMesh], []);
            DebugName = DebugName;
        }
    }
}
