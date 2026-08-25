using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TransportX
{
    public interface IMovable : IWorldObject
    {
        Vector3 Velocity { get; }
        Vector3 AngularVelocity { get; }
    }
}
