using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Input;

namespace TransportX.Sample.Vehicles.Interfaces
{
    internal class Pedal : IAxis
    {
        public Slider Source { get; set; }

        public float Rate => Source.Rate;

        public Pedal(Slider defaultSource)
        {
            Source = defaultSource;
        }
    }
}
