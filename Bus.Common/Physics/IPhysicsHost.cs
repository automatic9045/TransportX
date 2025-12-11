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

        void SetGroup(StaticHandle handle, ColliderGroupHandle group);
        void SetGroup(BodyHandle handle, ColliderGroupHandle group);
        void SetMaterial(StaticHandle handle, ColliderMaterial material);
        void SetMaterial(BodyHandle handle, ColliderMaterial material);
    }
}
