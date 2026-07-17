using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Communication;

namespace TransportX.Domains.RoadVehicles.Powertrain.Controllers
{
    public class ShifterAction
    {
        public Signal<int> InvokationCount { get; }

        public ShifterAction(Signal<int> invokationCount)
        {
            InvokationCount = invokationCount;
        }

        public void Invoke()
        {
            unchecked
            {
                InvokationCount.Value++;
            }
        }
    }
}
