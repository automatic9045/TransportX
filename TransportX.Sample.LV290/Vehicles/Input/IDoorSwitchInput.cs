using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Sample.Vehicles.Input
{
    internal interface IDoorSwitchInput
    {
        bool IsFrontOpen { get; }
        bool IsRearOpen { get; }
    }
}
