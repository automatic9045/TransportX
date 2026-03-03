using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Scripting.Commands;

namespace TransportX.Domains.RoadTraffic.Scripting.Commands
{
    public static class SplineFactoryCommandExtensions
    {
        public static SplineFactoryCommand SpeedLimit(this SplineFactoryCommand command, int pinIndex, double maxSpeed)
        {
            Templates.SplineSpeedLimit component = command.Components.GetOrAdd(() =>
            {
                int pinCount = command.SplineFactory.OutletLayout.Lanes.Count;
                return new Templates.SplineSpeedLimit(pinCount, command.ErrorCollector);
            });

            component.Add(pinIndex, (float)maxSpeed / 3.6f);

            return command;
        }
    }
}
