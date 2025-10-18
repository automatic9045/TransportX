using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuUtilities.Memory;

namespace Bus.Common.Physics
{
    public class PhysicsHost : IPhysicsHost, IDisposable
    {
        private readonly CollidableProperty<ColliderGroupHandle> Groups = new CollidableProperty<ColliderGroupHandle>();
        private readonly CollidableProperty<Material> Materials = new CollidableProperty<Material>();

        public Simulation Simulation { get; }

        protected PhysicsHost()
        {
            BufferPool bufferPool = new BufferPool();
            NarrowPhaseCallbacks narrowPhaseCallbacks = new NarrowPhaseCallbacks(Groups, Materials);
            Simulation = Simulation.Create(bufferPool, narrowPhaseCallbacks, new PoseIntegratorCallbacks(), new SolveDescription(1, 8));
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

        public void AddToGroup(BodyHandle body, ColliderGroupHandle group)
        {
            Groups.Allocate(body) = group;
        }
    }
}
