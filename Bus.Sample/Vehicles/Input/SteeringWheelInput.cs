using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Input;

namespace Bus.Sample.Vehicles.Input
{
    internal abstract class SteeringWheelInput : Slider
    {
        protected SteeringWheelInput() : base(-Spec.MaxSteeringWheelAngle, Spec.MaxSteeringWheelAngle)
        {
        }
    }
}
