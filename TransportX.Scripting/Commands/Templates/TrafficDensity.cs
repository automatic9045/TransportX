using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;
using TransportX.Network;

using TransportX.Extensions.Traffic;

namespace TransportX.Scripting.Components
{
    internal class TrafficDensity : ITemplateComponent<ILanePath>
    {
        public float Factor { get; }

        public TrafficDensity(float factor)
        {
            Factor = factor;
        }

        void ITemplateComponent<ILanePath>.Build(ILanePath path, IErrorCollector errorCollector)
        {
            TrafficDensityComponent compiled = new(Factor);
            path.Components.Add(compiled);
        }
    }
}
