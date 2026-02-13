using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;
using Vortice.Direct3D;
using Vortice.Direct3D11;

using TransportX.Rendering;

namespace TransportX.Physics.Colliders
{
    public class CylinderCollider : ColliderBase<Cylinder>
    {
        private const int SegmentCount = 16;


        public CylinderCollider(Cylinder shape, TypedIndex shapeIndex, ColliderMaterial material, Pose offset)
            : base(shape, shapeIndex, material, offset)
        {
        }

        public override BodyInertia ComputeInertia(float mass)
        {
            return Shape.ComputeInertia(mass);
        }

        public override IDebugModel CreateDebugModel(ID3D11Device device)
        {
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

            Material material = Rendering.Material.Default();
            Rendering.Mesh mesh = Rendering.Mesh.Create(device, vertices, indices, material, PrimitiveTopology.LineList);
            return new WireframeDebugModel([mesh]);
        }
    }
}
