using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Domains.RoadVehicles.Physics;
using TransportX.Domains.RoadVehicles.Powertrain.Constraints;

namespace TransportX.Domains.RoadVehicles.Powertrain.Modules
{
    public class Differential : IModule
    {
        public Shaft Input { get; }
        public Shaft LeftOutput { get; }
        public Shaft RightOutput { get; }
        public DifferentialConstraint Constraint { get; }

        public IReadOnlyList<Shaft> InputShafts { get; }
        public IReadOnlyList<Shaft> OutputShafts { get; }
        public IReadOnlyList<IConstraint> Constraints { get; }

        public Differential(Shaft input, Shaft leftOutput, Shaft rightOutput, float finalRatio)
        {
            Input = input;
            LeftOutput = leftOutput;
            RightOutput = rightOutput;

            Constraint = new DifferentialConstraint(Input, LeftOutput, RightOutput)
            {
                FinalRatio = finalRatio,
            };

            InputShafts = [Input];
            OutputShafts = [LeftOutput, RightOutput];
            Constraints = [Constraint];
        }

        public void Tick(TimeSpan elapsed)
        {
        }

        public void PropagateTorque()
        {
            Constraint.PropagateTorque(Input);
        }
    }
}
