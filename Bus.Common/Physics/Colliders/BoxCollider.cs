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
    public class BoxCollider : Collider<Box>
    {
        protected readonly Material DebugMaterial = new(Vector4.One, []);
        public override Vector4 DebugColor
        {
            get => DebugMaterial.BaseColor;
            set => DebugMaterial.BaseColor = value;
        }

        public BoxCollider(Box shape, TypedIndex shapeIndex, ColliderMaterial material, Pose offset)
            : base(shape, shapeIndex, material, offset, (s, m) => s.ComputeInertia(m))
        {
        }

        public override void CreateDebugResources(ID3D11Device device)
        {
            if (DebugModel is not null) throw new InvalidOperationException("モデルは既に作成されています。");

            Vertex[] vertices = [
                new(new Vector3(-Shape.HalfWidth, -Shape.HalfHeight, -Shape.HalfLength), Vector4.One),
                new(new Vector3( Shape.HalfWidth, -Shape.HalfHeight, -Shape.HalfLength), Vector4.One),
                new(new Vector3(-Shape.HalfWidth,  Shape.HalfHeight, -Shape.HalfLength), Vector4.One),
                new(new Vector3( Shape.HalfWidth,  Shape.HalfHeight, -Shape.HalfLength), Vector4.One),
                new(new Vector3(-Shape.HalfWidth, -Shape.HalfHeight,  Shape.HalfLength), Vector4.One),
                new(new Vector3( Shape.HalfWidth, -Shape.HalfHeight,  Shape.HalfLength), Vector4.One),
                new(new Vector3(-Shape.HalfWidth,  Shape.HalfHeight,  Shape.HalfLength), Vector4.One),
                new(new Vector3( Shape.HalfWidth,  Shape.HalfHeight,  Shape.HalfLength), Vector4.One),
            ];

            int[] indices = [
                0, 1, 1, 3, 3, 2, 2, 0,
                4, 5, 5, 7, 7, 6, 6, 4,
                0, 4, 1, 5, 2, 6, 3, 7,
            ];

            Rendering.Mesh visualMesh = Rendering.Mesh.Create(device, vertices, indices, DebugMaterial, PrimitiveTopology.LineList);
            DebugModel = new Model([visualMesh], []);
            DebugName = DebugName;
        }
    }
}
