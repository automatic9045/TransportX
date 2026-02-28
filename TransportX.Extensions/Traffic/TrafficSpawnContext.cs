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
        public required ICollection<RigidBody> Bodies { get; init; }
        public required IEnumerable<ITrafficParticipant> Obstacles { get; init; }

        public TrafficSpawnContext()
        {
        }
    }
}
