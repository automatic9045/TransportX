using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;
using TransportX.Network;

using CompiledComponent = TransportX.Extensions.Traffic.TrafficDensityComponent;

namespace TransportX.Scripting.Components
{
    public class TrafficDensityComponent : ITemplateComponent<ILanePath>
    {
        public float Factor { get; }

        public TrafficDensityComponent(float factor)
        {
            Factor = factor;
        }

        void ITemplateComponent<ILanePath>.Build(ILanePath path, IErrorCollector errorCollector)
        {
            CompiledComponent compiled = new(Factor);
            path.Components.Add(compiled);
        }
    }
}
