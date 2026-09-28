using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Spatial;

namespace TransportX.Bodies
{
    public interface IBodyCollection : IReadOnlyList<RigidBody>
    {
        event Action<RigidBody>? Added;
        event Action<RigidBody>? Removed;

        void Add(RigidBody body);
        bool Remove(RigidBody body);
    }
}
