using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Input;

namespace TransportX.Sample.LV290.Vehicles.Input
{
    internal abstract class SteeringWheelInput : Slider
    {
        protected SteeringWheelInput() : base(-Spec.MaxSteeringWheelAngle, Spec.MaxSteeringWheelAngle)
        {
        }
    }
}
