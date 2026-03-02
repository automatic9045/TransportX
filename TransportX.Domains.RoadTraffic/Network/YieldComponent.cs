using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;
using TransportX.Network;

namespace TransportX.Domains.RoadTraffic.Network
{
    public class YieldComponent : IComponent
    {
        public IReadOnlyList<ILanePath> PriorityPaths { get; }

        public YieldComponent(IReadOnlyList<ILanePath> priorityPaths)
        {
            PriorityPaths = priorityPaths;
        }
    }
}
