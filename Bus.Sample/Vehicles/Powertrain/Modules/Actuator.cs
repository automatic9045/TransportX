using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Sample.Vehicles.Interfaces;

namespace Bus.Sample.Vehicles.Powertrain.Modules
{
    internal class Actuator : IAxis
    {
        public float Rate
        {
            get => field;
            set => field = float.Clamp(value, 0, 1);
        } = 0;
    }
}
