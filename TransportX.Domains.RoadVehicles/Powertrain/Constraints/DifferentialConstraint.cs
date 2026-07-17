using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Domains.RoadVehicles.Physics;

namespace TransportX.Domains.RoadVehicles.Powertrain.Constraints
{
    public class DifferentialConstraint : LinearConstraint
    {
        private readonly Shaft Input;
        private readonly Shaft LeftOutput;
        private readonly Shaft RightOutput;

        public required float FinalRatio
        {
            get => field;
            set
            {
                if (field == value) return;
                field = value;

                SetCoefficient(Input, -1 / value);
                Build();
            }
        }

        public DifferentialConstraint(Shaft input, Shaft leftOutput, Shaft rightOutput) : base(input, leftOutput, rightOutput)
        {
            Input = input;
            LeftOutput = leftOutput;
            RightOutput = rightOutput;

            SetCoefficient(LeftOutput, 0.5f);
            SetCoefficient(RightOutput, 0.5f);
        }
    }
}
