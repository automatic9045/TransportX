using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Network;

namespace TransportX.Extensions.Traffic
{
    public class TrafficSpawnerCollection
    {
        private readonly List<ITrafficSpawner> Items = [];

        public TrafficSpawnerCollection()
        {
        }

        public void Register(ITrafficSpawner spawner)
        {
            Items.Add(spawner);
        }

        public void Initialize(IEnumerable<NetworkElement> network)
        {
            ILanePath[] paths = network
                .SelectMany(element => element.Paths)
                .ToArray();

            NetworkPort[] sourcePorts = network
                .SelectMany(element => element.Ports)
                .Where(port => !port.IsConnected)
                .ToArray();

            foreach (ITrafficSpawner item in Items) item.Initialize(paths, sourcePorts);
        }

        public void Tick(TimeSpan elapsed)
        {
            foreach (ITrafficSpawner item in Items) item.Tick(elapsed);
        }
    }
}
