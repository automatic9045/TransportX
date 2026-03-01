using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;
using ColliderMesh = BepuPhysics.Collidables.Mesh;
using Vortice.Direct3D;
using Vortice.Direct3D11;

using TransportX.Rendering;

namespace TransportX.Physics.Colliders
{
    public class MeshCollider : ColliderBase<ColliderMesh>
    {
        public bool IsOpen { get; }

        public MeshCollider(Simulation simulation, ColliderMesh shape, ColliderMaterial material, Pose offset, bool isOpen)
            : base(simulation, shape, material, offset)
        {
            IsOpen = isOpen;
        }

        public override BodyInertia ComputeInertia(float mass)
        {
            return IsOpen ? Shape.ComputeOpenInertia(mass) : Shape.ComputeClosedInertia(mass);
        }

        public override IDebugModel CreateDebugModel(ID3D11Device device)
        {
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

            Material material = Rendering.Material.Default();
            Rendering.Mesh mesh = Rendering.Mesh.Create(device, vertices, indices, material, PrimitiveTopology.LineList);
            return new WireframeDebugModel([mesh]);
        }
    }
}
