using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Sample.LV290.Vehicles.Powertrain.Constraints;
using TransportX.Sample.LV290.Vehicles.Powertrain.Physics;

namespace TransportX.Sample.LV290.Vehicles.Powertrain.Modules
{
    internal class Differential : IModule
    {
        private readonly Shaft Input;
        private readonly Shaft LeftOutput;
        private readonly Shaft RightOutput;

        private readonly DifferentialConstraint Constraint;

        public IEnumerable<IConstraint> Constraints { get; }

        public Differential(Shaft input, Shaft leftOutput, Shaft rightOutput)
        {
            Input = input;
            LeftOutput = leftOutput;
            RightOutput = rightOutput;

            Constraint = new DifferentialConstraint(Input, LeftOutput, RightOutput);
            Constraints = [Constraint];
        }

        public void PropagateTorque()
        {
            Constraint.PropagateTorque(Input);
        }
    }
}
