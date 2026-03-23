using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;

namespace TransportX.Physics
{
    public class PhysicsHost : IPhysicsHost, IDisposable
    {
        private readonly CollidableProperty<ColliderGroupHandle> Groups = new();
        private readonly CollidableProperty<ColliderMaterial> Materials = new();

        private BufferPool BufferPool;
        private readonly ThreadDispatcher ThreadDispatcherKey;

        private bool IsDisposed = false;

        public Simulation Simulation { get; }
        public IThreadDispatcher ThreadDispatcher => ThreadDispatcherKey;

        protected PhysicsHost()
        {
            BufferPool = new BufferPool();
            NarrowPhaseCallbacks narrowPhaseCallbacks = new(Groups, Materials);
            Simulation = Simulation.Create(BufferPool, narrowPhaseCallbacks, new PoseIntegratorCallbacks(), new SolveDescription(1, 8));
            ThreadDispatcherKey = new ThreadDispatcher(System.Environment.ProcessorCount);
        }

        internal static PhysicsHost Create()
        {
            return new PhysicsHost();
        }

        public void Dispose()
        {
            if (IsDisposed) throw new InvalidOperationException();
            IsDisposed = true;

            Simulation.Dispose();
            ThreadDispatcherKey.Dispose();
            BufferPool.Clear();
        }

        public void SetGroup(StaticHandle handle, ColliderGroupHandle group)
        {
            Groups.Allocate(handle) = group;
        }

        public void SetGroup(BodyHandle handle, ColliderGroupHandle group)
        {
            Groups.Allocate(handle) = group;
        }

        public void SetMaterial(StaticHandle handle, ColliderMaterial material)
        {
            Materials.Allocate(handle) = material;
        }

        public void SetMaterial(BodyHandle handle, ColliderMaterial material)
        {
            Materials.Allocate(handle) = material;
        }
    }
}
