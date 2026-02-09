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
    public class SphereCollider : ColliderBase<Sphere>
    {
        private const int SegmentCount = 16;


        public SphereCollider(Sphere shape, TypedIndex shapeIndex, ColliderMaterial material, Pose offset)
            : base(shape, shapeIndex, material, offset)
        {
        }

        public override BodyInertia ComputeInertia(float mass)
        {
            return Shape.ComputeInertia(mass);
        }

        public override IDebugModel CreateDebugModel(ID3D11Device device)
        {
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

            Material material = new(Vector4.One, []);
            Rendering.Mesh mesh = Rendering.Mesh.Create(device, vertices, indices, material, PrimitiveTopology.LineList);
            return new WireframeDebugModel([mesh]);
        }
    }
}
