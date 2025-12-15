using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Sample.Vehicles.Powertrain.Physics;

namespace Bus.Sample.Vehicles.Powertrain.Constraints
{
    internal class TransmissionConstraint : LinearConstraint
    {
        private readonly Shaft Input;
        private readonly Shaft Output;

        private float RatioKey;
        public float Ratio
        {
            get => RatioKey;
            set
            {
                if (Ratio == value) return;
                RatioKey = value;

                SetCoefficient(Output, Ratio);
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
