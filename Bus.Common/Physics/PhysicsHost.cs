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
        private readonly CollidableProperty<Material> Materials = new CollidableProperty<Material>();

        public BufferPool BufferPool { get; }
        public Simulation Simulation { get; }

        protected PhysicsHost()
        {
            BufferPool = new BufferPool();
            Simulation = Simulation.Create(BufferPool, new NarrowPhaseCallbacks(Materials), new PoseIntegratorCallbacks(), new SolveDescription(1, 8));
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
