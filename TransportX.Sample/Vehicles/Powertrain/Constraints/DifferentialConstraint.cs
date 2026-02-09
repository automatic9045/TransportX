using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Sample.Vehicles.Powertrain.Physics;

namespace TransportX.Sample.Vehicles.Powertrain.Constraints
{
    internal class DifferentialConstraint : LinearConstraint
    {
        private static readonly float FinalRatio = 6.5f;


        private readonly Shaft Input;
        private readonly Shaft LeftOutput;
        private readonly Shaft RightOutput;

        public DifferentialConstraint(Shaft input, Shaft leftOutput, Shaft rightOutput) : base(input, leftOutput, rightOutput)
        {
            Input = input;
            LeftOutput = leftOutput;
            RightOutput = rightOutput;

            SetCoefficient(Input, -1 / FinalRatio);
            SetCoefficient(LeftOutput, 0.5f);
            SetCoefficient(RightOutput, 0.5f);
            Build();
        }
    }
}
