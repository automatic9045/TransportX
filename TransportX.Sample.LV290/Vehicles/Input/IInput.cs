using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Input;

namespace TransportX.Sample.LV290.Vehicles.Input
{
    internal interface IInput
    {
        Slider Clutch { get; }
        Slider Brake { get; }
        Slider Throttle { get; }
        SteeringWheelInput Steering { get; }
        IATShifterInput ATShifter { get; }
        IMTShifterInput MTShifter { get; }

        IDoorSwitchInput DoorSwitch { get; }

        void Tick(TimeSpan elapsed);
    }
}
