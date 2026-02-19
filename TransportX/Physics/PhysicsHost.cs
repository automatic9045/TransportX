using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuUtilities;
using BepuUtilities.Memory;

namespace TransportX.Physics
{
    public class PhysicsHost : IPhysicsHost, IDisposable
    {
        private readonly CollidableProperty<ColliderGroupHandle> Groups = new CollidableProperty<ColliderGroupHandle>();
        private readonly CollidableProperty<ColliderMaterial> Materials = new CollidableProperty<ColliderMaterial>();

        public Simulation Simulation { get; }
        public IThreadDispatcher ThreadDispatcher { get; }

        protected PhysicsHost()
        {
            BufferPool bufferPool = new BufferPool();
            NarrowPhaseCallbacks narrowPhaseCallbacks = new NarrowPhaseCallbacks(Groups, Materials);
            Simulation = Simulation.Create(bufferPool, narrowPhaseCallbacks, new PoseIntegratorCallbacks(), new SolveDescription(1, 8));
            ThreadDispatcher = new ThreadDispatcher(System.Environment.ProcessorCount);
        }

        internal static PhysicsHost Create()
        {
            return new PhysicsHost();
        }

        public void Dispose()
        {
            Groups.Dispose();
            Materials.Dispose();

            BufferPool bufferPool = Simulation.BufferPool;
            Simulation.Dispose();
            bufferPool.Clear();
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
