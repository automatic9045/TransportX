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
        private readonly List<TrafficSpawner> Items = [];

        public TrafficSpawnerCollection()
        {
        }

        public void Register(TrafficSpawner spawner)
        {
            Items.Add(spawner);
        }

        public void Initialize(IEnumerable<NetworkElement> network)
        {
            IEnumerable<ILanePath> paths = network.SelectMany(element => element.Paths);

            IEnumerable<NetworkPort> sourcePorts = network
                .SelectMany(element => element.Ports)
                .Where(port => !port.IsConnected);

            foreach (TrafficSpawner item in Items) item.Initialize(paths, sourcePorts);
        }

        public void Tick(TimeSpan elapsed)
        {
            foreach (TrafficSpawner item in Items) item.Tick(elapsed);
        }
    }
}
