using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using Vortice.Direct3D;
using Vortice.Direct3D11;

using TransportX.Rendering;

namespace TransportX.Physics.Colliders
{
    public class ConvexHullCollider : ColliderBase<ConvexHull>
    {
        public ConvexHullCollider(ConvexHull shape, TypedIndex shapeIndex, ColliderMaterial material, Pose offset)
            : base(shape, shapeIndex, material, offset)
        {
        }

        public override BodyInertia ComputeInertia(float mass)
        {
            return Shape.ComputeInertia(mass);
        }

        public override IDebugModel CreateDebugModel(ID3D11Device device)
        {
            List<Vector3> extractedPoints = [];
            for (int i = 0; i < Shape.Points.Length; i++)
            {
                Vector3Wide widePoint = Shape.Points[i];
                for (int j = 0; j < Vector<float>.Count; j++)
                {
                    Vector3 p = new(widePoint.X[j], widePoint.Y[j], widePoint.Z[j]);
                    extractedPoints.Add(p);
                }
            }

            Vertex[] vertices = new Vertex[extractedPoints.Count + 1];
            vertices[0] = new Vertex(Vector3.Zero, Vector4.One);

            for (int i = 0; i < extractedPoints.Count; i++)
            {
                vertices[i + 1] = new Vertex(extractedPoints[i], Vector4.One);
            }

            int[] indices = new int[extractedPoints.Count * 2];
            for (int i = 0; i < extractedPoints.Count; i++)
            {
                int baseIndex = i * 2;
                indices[baseIndex] = 0;
                indices[baseIndex + 1] = i + 1;
            }

            Material material = Rendering.Material.Default();
            Rendering.Mesh mesh = Rendering.Mesh.Create(device, vertices, indices, material, PrimitiveTopology.LineList);
            return new WireframeDebugModel([mesh]);
        }
    }
}
