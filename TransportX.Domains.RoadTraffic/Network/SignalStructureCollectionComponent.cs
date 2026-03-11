using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;
using TransportX.Spatial;

namespace TransportX.Domains.RoadTraffic.Network
{
    public class SignalStructureCollectionComponent : ITickableComponent
    {
        private readonly IReadOnlyList<SignalStructure> Structures;

        public SignalStructureCollectionComponent(IReadOnlyList<SignalStructure> structures)
        {
            Structures = structures;
        }

        public void Tick(TimeSpan elapsed, DateTime now)
        {
            for (int i = 0; i < Structures.Count; i++)
            {
                Structures[i].Tick(now);
            }
        }
    }
}
