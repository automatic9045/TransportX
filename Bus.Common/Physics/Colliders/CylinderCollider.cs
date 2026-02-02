using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics.Collidables;
using BepuUtilities;
using Vortice.Direct3D;
using Vortice.Direct3D11;

using Bus.Common.Rendering;

namespace Bus.Common.Physics.Colliders
{
    public class CylinderCollider : Collider<Cylinder>
    {
        private const int SegmentCount = 16;


        protected readonly Material DebugMaterial = new(Vector4.One, []);
        public override Vector4 DebugColor
        {
            get => DebugMaterial.BaseColor;
            set => DebugMaterial.BaseColor = value;
        }

        public CylinderCollider(Cylinder shape, TypedIndex shapeIndex, ColliderMaterial material, Pose offset)
            : base(shape, shapeIndex, material, offset, (s, m) => s.ComputeInertia(m))
        {
        }

        public override void CreateDebugResources(ID3D11Device device)
        {
            if (DebugModel is not null) throw new InvalidOperationException("モデルは既に作成されています。");

            float angleStep = float.Pi * 2 / SegmentCount;
            Vertex[] vertices = new Vertex[SegmentCount * 2];

            for (int i = 0; i < SegmentCount; i++)
            {
                float angle = i * angleStep;
                float x = float.Cos(angle) * Shape.Radius;
                float z = float.Sin(angle) * Shape.Radius;
                vertices[i] = new Vertex(new Vector3(x, Shape.HalfLength, z), Vector4.One);
            }

            for (int i = 0; i < SegmentCount; i++)
            {
                float angle = i * angleStep;
                float x = float.Cos(angle) * Shape.Radius;
                float z = float.Sin(angle) * Shape.Radius;
                vertices[SegmentCount + i] = new Vertex(new Vector3(x, -Shape.HalfLength, z), Vector4.One);
            }

            int[] indices = new int[SegmentCount * 6];
            int index = 0;

            for (int i = 0; i < SegmentCount; i++)
            {
                int next = (i + 1) % SegmentCount;

                indices[index++] = i;
                indices[index++] = next;

                indices[index++] = SegmentCount + i;
                indices[index++] = SegmentCount + next;

                indices[index++] = i;
                indices[index++] = SegmentCount + i;
            }

            Rendering.Mesh visualMesh = Rendering.Mesh.Create(device, vertices, indices, DebugMaterial, PrimitiveTopology.LineList);
            DebugModel = new Model([visualMesh], []);
            DebugName = DebugName;
        }
    }
}
