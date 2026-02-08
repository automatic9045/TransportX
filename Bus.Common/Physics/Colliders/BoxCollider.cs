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

using Bus.Common.Rendering;

namespace Bus.Common.Physics.Colliders
{
    public class BoxCollider : ColliderBase<Box>
    {
        public BoxCollider(Box shape, TypedIndex shapeIndex, ColliderMaterial material, Pose offset)
            : base(shape, shapeIndex, material, offset)
        {
        }

        public override BodyInertia ComputeInertia(float mass)
        {
            return Shape.ComputeInertia(mass);
        }

        public override IDebugModel CreateDebugModel(ID3D11Device device)
        {
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

            Material material = new(Vector4.One, []);
            Rendering.Mesh mesh = Rendering.Mesh.Create(device, vertices, indices, material, PrimitiveTopology.LineList);
            return new WireframeDebugModel([mesh]);
        }
    }
}
