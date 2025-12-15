using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Sample.Vehicles.Powertrain.Physics;

namespace Bus.Sample.Vehicles.Powertrain.Constraints
{
    internal class ClutchConstraint : LinearConstraint
    {
        private readonly Shaft Input;
        private readonly Shaft Output;

        public ClutchConstraint(Shaft input, Shaft output) : base(input, output)
        {
            Input = input;
            Output = output;

            SetCoefficient(Input, 1);
            SetCoefficient(Output, -1);
            Build();
        }
    }
}
