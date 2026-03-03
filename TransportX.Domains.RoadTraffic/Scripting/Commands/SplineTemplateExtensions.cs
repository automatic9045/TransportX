using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Scripting.Commands;

namespace TransportX.Domains.RoadTraffic.Scripting.Commands
{
    public static class SplineTemplateExtensions
    {
        public static SplineTemplate SpeedLimit(this SplineTemplate template, int pinIndex, double maxSpeed)
        {
            Templates.SplineSpeedLimit component = template.Components.GetOrAdd(() =>
            {
                int pinCount = template.OutletLayout.Lanes.Count;
                return new Templates.SplineSpeedLimit(pinCount, template.ErrorCollector);
            });

            component.Add(pinIndex, (float)maxSpeed / 3.6f);

            return template;
        }
    }
}
