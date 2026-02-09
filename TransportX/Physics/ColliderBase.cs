using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;
using Vortice.Direct3D11;

using TransportX.Rendering;

namespace TransportX.Physics
{
    public abstract class ColliderBase<TShape> : ICollider where TShape : unmanaged, IShape
    {
        public TShape Shape { get; }
        IShape ICollider.Shape => Shape;
        public TypedIndex ShapeIndex { get; }
        public ColliderMaterial Material { get; }
        public Pose Offset { get; }
        public Pose OffsetInverse { get; }

        public ColliderBase(TShape shape, TypedIndex shapeIndex, ColliderMaterial material, Pose offset)
        {
            Shape = shape;
            ShapeIndex = shapeIndex;
            Material = material;
            Offset = offset;
            OffsetInverse = Pose.Inverse(Offset);
        }

        public abstract BodyInertia ComputeInertia(float mass);
        public abstract IDebugModel CreateDebugModel(ID3D11Device device);
    }
}
