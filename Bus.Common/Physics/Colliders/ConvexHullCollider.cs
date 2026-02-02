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
    public class ConvexHullCollider : Collider<ConvexHull>
    {
        protected readonly Material DebugMaterial = new(Vector4.One, []);
        public override Vector4 DebugColor
        {
            get => DebugMaterial.BaseColor;
            set => DebugMaterial.BaseColor = value;
        }

        public ConvexHullCollider(ConvexHull shape, TypedIndex shapeIndex, ColliderMaterial material, Pose offset)
            : base(shape, shapeIndex, material, offset, (s, m) => s.ComputeInertia(m))
        {
        }

        public override void CreateDebugResources(ID3D11Device device)
        {
            if (DebugModel is not null) throw new InvalidOperationException("モデルは既に作成されています。");

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

            Rendering.Mesh visualMesh = Rendering.Mesh.Create(device, vertices, indices, DebugMaterial, PrimitiveTopology.LineList);
            DebugModel = new Model([visualMesh], []);
            DebugName = DebugName;
        }
    }
}
