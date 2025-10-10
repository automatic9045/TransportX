using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Sample.Vehicles.Drives
{
    internal enum DoorInterlockModes
    {
        None = 0,
        Clutch = 0b1,
        Brake = 0b10,
        Throttle = 0b100,
    }
}
