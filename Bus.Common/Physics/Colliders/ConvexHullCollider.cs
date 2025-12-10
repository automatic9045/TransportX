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
using BepuUtilities;

namespace Bus.Common.Physics.Colliders
{
    public class ConvexHullCollider : Collider<ConvexHull>
    {
        public ConvexHullCollider(ConvexHull shape, TypedIndex shapeIndex, Material material, Matrix4x4 offset)
            : base(shape, shapeIndex, material, offset, (s, m) => s.ComputeInertia(m))
        {
        }

        public override void CreateDebugModel(ID3D11Device device, Vector4 color)
        {
            if (DebugModel is not null) throw new InvalidOperationException("モデルは既に作成されています。");

            List<Vector3> extractedPoints = new List<Vector3>();
            for (int i = 0; i < Shape.Points.Length; i++)
            {
                Vector3Wide widePoint = Shape.Points[i];
                for (int j = 0; j < Vector<float>.Count; j++)
                {
                    Vector3 p = new Vector3(widePoint.X[j], widePoint.Y[j], widePoint.Z[j]);
                    extractedPoints.Add(p);
                }
            }

            Vertex[] vertices = new Vertex[extractedPoints.Count + 1];
            vertices[0] = new Vertex(Vector3.Zero, color);

            for (int i = 0; i < extractedPoints.Count; i++)
            {
                vertices[i + 1] = new Vertex(extractedPoints[i], color);
            }

            int[] indices = new int[Shape.Points.Length * 2];
            for (int i = 0; i < Shape.Points.Length; i++)
            {
                int baseIndex = i * 2;
                indices[baseIndex] = 0;
                indices[baseIndex + 1] = i + 1;
            }

            Rendering.Mesh visualMesh = Rendering.Mesh.Create(device, vertices, indices, [], PrimitiveTopology.LineList);
            DebugModel = new Model([visualMesh], []);
        }
    }
}
