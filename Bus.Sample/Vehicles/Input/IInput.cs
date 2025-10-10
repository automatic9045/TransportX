using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Input;

namespace Bus.Sample.Vehicles.Input
{
    internal interface IInput
    {
        Slider Clutch { get; }
        Slider Brake { get; }
        Slider Throttle { get; }
        SteeringInput Steering { get; }
        IATShifterInput ATShifter { get; }

        void Tick(TimeSpan elapsed);
    }
}
