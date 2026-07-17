using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Domains.RoadVehicles.Physics;

namespace TransportX.Domains.RoadVehicles.Powertrain.Constraints
{
    public class DirectConstraint : LinearConstraint
    {
        private readonly Shaft Input;
        private readonly Shaft Output;

        public DirectConstraint(Shaft input, Shaft output) : base(input, output)
        {
            Input = input;
            Output = output;

            SetCoefficient(Input, 1);
            SetCoefficient(Output, -1);
            Build();
        }
    }
}
