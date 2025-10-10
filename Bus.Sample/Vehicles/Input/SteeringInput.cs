using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Input;

namespace Bus.Sample.Vehicles.Input
{
    internal abstract class SteeringInput : Slider
    {
        public double SteeringTorque { get; set; } = 0;

        protected SteeringInput() : base(-2.5 * double.Pi, 2.5 * double.Pi)
        {
        }
    }
}
