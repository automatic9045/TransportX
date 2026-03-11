using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;
using TransportX.Worlds;

namespace TransportX.Domains.RoadTraffic.Network
{
    public class SignalCollectionComponent : ITickableComponent
    {
        private readonly WorldBase World;

        public List<SignalController> Controllers { get; } = [];

        public SignalCollectionComponent(WorldBase world)
        {
            World = world;
        }

        public void Tick(TimeSpan elapsed, DateTime now)
        {
            foreach (SignalController controller in Controllers)
            {
                controller.Tick(now);
            }
        }
    }
}
