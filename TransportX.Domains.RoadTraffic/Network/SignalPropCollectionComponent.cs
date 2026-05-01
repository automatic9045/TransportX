using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;

namespace TransportX.Domains.RoadTraffic.Network
{
    public class SignalPropCollectionComponent : ITickableComponent
    {
        private readonly IReadOnlyList<SignalProp> Props;

        public SignalPropCollectionComponent(IReadOnlyList<SignalProp> props)
        {
            Props = props;
        }

        public void Tick(TimeSpan elapsed, DateTime now)
        {
            for (int i = 0; i < Props.Count; i++)
            {
                Props[i].Tick(now);
            }
        }
    }
}
