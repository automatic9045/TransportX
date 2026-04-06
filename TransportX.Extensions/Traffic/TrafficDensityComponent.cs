using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;
using TransportX.Diagnostics;
using TransportX.Network;

namespace TransportX.Extensions.Traffic
{
    public class TrafficDensityComponent : IComponent
    {
        public float Factor { get; }

        public TrafficDensityComponent(float factor)
        {
            Factor = factor;
        }
    }
}
