using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.CollisionDetection;
using BepuUtilities.Memory;

namespace Bus.Common.Physics
{
    public class PhysicsHost : IPhysicsHost, IDisposable
    {
        public BufferPool BufferPool { get; }
        public CollidableProperty<Material> Materials { get; }
        public INarrowPhaseCallbacks NarrowPhaseCallbacks { get; }
        public IPoseIntegratorCallbacks PoseIntegratorCallbacks { get; }
        public Simulation Simulation { get; }

        protected PhysicsHost()
        {
            BufferPool = new BufferPool();
            Materials = new CollidableProperty<Material>();
            NarrowPhaseCallbacks = new NarrowPhaseCallbacks(Materials);
            PoseIntegratorCallbacks = new PoseIntegratorCallbacks();
            Simulation = Simulation.Create(BufferPool,
                (NarrowPhaseCallbacks)NarrowPhaseCallbacks, (PoseIntegratorCallbacks)PoseIntegratorCallbacks,new SolveDescription(1, 8));
        }

        internal static PhysicsHost Create()
        {
            return new PhysicsHost();
        }

        public void Dispose()
        {
            Simulation.Dispose();
            BufferPool.Clear();
        }
    }
}
