using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;
using TransportX.Network;
using TransportX.Worlds;

namespace TransportX.Extensions.Traffic
{
    public class TrafficSpawnerComponent : IStartableComponent, ITickableComponent
    {
        private readonly WorldBase World;

        public TrafficSpawnerCollection Spawners { get; } = new();

        public TrafficSpawnerComponent(WorldBase world)
        {
            World = world;
        }

        public void OnStart()
        {
            IEnumerable<NetworkElement> network = World.Plates.SelectMany(plate => plate.Network);
            Spawners.Initialize(network);
        }

        public void Tick(TimeSpan elapsed, DateTime now)
        {
            Spawners.Tick(elapsed);
        }
    }
}
