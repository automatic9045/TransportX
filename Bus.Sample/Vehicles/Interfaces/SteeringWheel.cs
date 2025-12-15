using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Sample.Vehicles.Input;

namespace Bus.Sample.Vehicles.Interfaces
{
    internal class SteeringWheel
    {
        public SteeringWheelInput Source { get; set; }

        public float Rate => Source.Rate;

        public SteeringWheel(SteeringWheelInput defaultSource)
        {
            Source = defaultSource;
        }
    }
}
