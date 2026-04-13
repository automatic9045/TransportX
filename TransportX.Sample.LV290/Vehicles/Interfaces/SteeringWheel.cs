using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Sample.LV290.Vehicles.Input;

namespace TransportX.Sample.LV290.Vehicles.Interfaces
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
