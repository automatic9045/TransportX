using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Bodies;
using TransportX.Physics;
using TransportX.Traffic;

namespace TransportX.Extensions.Traffic
{
    public readonly struct TrafficSpawnContext
    {
        public required IPhysicsHost PhysicsHost { get; init; }
        public required IBodyCollection Bodies { get; init; }
        public required ITrafficRegistry Registry { get; init; }

        public TrafficSpawnContext()
        {
        }
    }
}
