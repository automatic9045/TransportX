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
        protected readonly Simulation Simulation;

        public TShape Shape { get; }
        IShape ICollider.Shape => Shape;
        public TypedIndex ShapeIndex { get; }
        public ColliderMaterial Material { get; }
        public Pose Offset { get; }
        public Pose OffsetInverse { get; }

        public ColliderBase(Simulation simulation, TShape shape, ColliderMaterial material, Pose offset)
        {
            Simulation = simulation;
            Shape = shape;
            ShapeIndex = simulation.Shapes.Add(shape);
            Material = material;
            Offset = offset;
            OffsetInverse = Pose.Inverse(Offset);
        }

        public virtual void Dispose()
        {
            Simulation.Shapes.RemoveAndDispose(ShapeIndex, Simulation.BufferPool);
        }

        public abstract BodyInertia ComputeInertia(float mass);
        public abstract IDebugModel CreateDebugModel(ID3D11Device device);
    }
}
