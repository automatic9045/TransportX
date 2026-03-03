using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;
using TransportX.Network;

using TransportX.Scripting;

namespace TransportX.Domains.RoadTraffic.Network
{
    public class SpeedLimitComponent : ITemplateComponent<ILanePath>
    {
        public float MaxSpeed { get; }

        public SpeedLimitComponent(float maxSpeed)
        {
            MaxSpeed = maxSpeed;
        }

        void ITemplateComponent<ILanePath>.Build(ILanePath parent, IErrorCollector errorCollector)
        {
            parent.Components.Add(this);
        }
    }
}
