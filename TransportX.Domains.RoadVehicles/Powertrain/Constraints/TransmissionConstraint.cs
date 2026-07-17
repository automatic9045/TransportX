using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Domains.RoadVehicles.Physics;

namespace TransportX.Domains.RoadVehicles.Powertrain.Constraints
{
    public class TransmissionConstraint : LinearConstraint
    {
        private readonly Shaft Input;
        private readonly Shaft Output;

        public float Ratio
        {
            get => field;
            set
            {
                if (field == value) return;
                field = value;

                SetCoefficient(Output, value);
                Build();
            }
        }

        public TransmissionConstraint(Shaft input, Shaft output) : base(input, output)
        {
            Input = input;
            Output = output;

            SetCoefficient(Input, -1);
            Ratio = 1;
        }
    }
}
