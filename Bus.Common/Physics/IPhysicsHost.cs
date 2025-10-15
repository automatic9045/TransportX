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
    public interface IPhysicsHost
    {
        BufferPool BufferPool { get; }
        Simulation Simulation { get; }
    }
}
