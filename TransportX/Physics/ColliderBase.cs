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

        private bool IsDisposed = false;

        public TShape Shape { get; }
        IShape ICollider.Shape => Shape;
        public TypedIndex ShapeIndex { get; }
        public ColliderMaterial Material { get; }
        public Pose Offset { get; }
        public Pose OffsetInverse { get; }

        public IDebugModel? DebugModel { get; private set; } = null;

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
            if (IsDisposed) throw new InvalidOperationException();
            IsDisposed = true;

            Simulation.Shapes.RemoveAndDispose(ShapeIndex, Simulation.BufferPool);
        }

        public void CreateDebugModel(ID3D11Device device)
        {
            DebugModel ??= CreateDebugModelCore(device);
        }

        public abstract BodyInertia ComputeInertia(float mass);
        protected abstract IDebugModel CreateDebugModelCore(ID3D11Device device);
    }
}
