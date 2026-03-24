using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Sample.Vehicles.Input;

namespace TransportX.Sample.Vehicles.Interfaces
{
    internal class DoorSwitch
    {
        public IDoorSwitchInput Source { get; set; }

        public bool IsFrontOpen => Source.IsFrontOpen;
        public bool IsRearOpen => Source.IsRearOpen;

        public DoorSwitch(IDoorSwitchInput defaultSource)
        {
            Source = defaultSource;
        }
    }
}
