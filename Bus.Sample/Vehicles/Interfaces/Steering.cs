using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Sample.Vehicles.Input;

namespace Bus.Sample.Vehicles.Interfaces
{
    internal class Steering
    {
        public SteeringInput Source { get; set; }

        public double Rate => Source.Rate;

        public Steering(SteeringInput defaultSource)
        {
            Source = defaultSource;
        }

        public void Tick(TimeSpan elapsed)
        {

        }
    }
}
