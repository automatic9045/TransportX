using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;

namespace Bus.Common.Physics
{
    public interface IPhysicsHost
    {
        Simulation Simulation { get; }

        void SetGroup(BodyHandle body, ColliderGroupHandle group);
    }
}
