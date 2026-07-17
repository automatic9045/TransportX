using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Domains.RoadVehicles.Physics;
using TransportX.Domains.RoadVehicles.Powertrain.Constraints;

namespace TransportX.Domains.RoadVehicles.Powertrain.Modules
{
    public abstract class ClutchBase : IModule
    {
        public Shaft Input { get; }
        public Shaft Output { get; }
        public DirectConstraint Constraint { get; }

        public IReadOnlyList<Shaft> InputShafts { get; }
        public IReadOnlyList<Shaft> OutputShafts { get; }
        public IReadOnlyList<IConstraint> Constraints { get; }

        public float Engagement
        {
            get => field;
            set => field = float.Clamp(value, 0, 1);
        }

        protected ClutchBase(Shaft input, Shaft output)
        {
            Input = input;
            Output = output;
            Constraint = new DirectConstraint(Input, Output);

            InputShafts = [Input];
            OutputShafts = [Output];
            Constraints = [Constraint];
        }

        public abstract void Tick(TimeSpan elapsed);

        public void PropagateTorque()
        {
            Constraint.PropagateTorque(Input);
        }
    }
}
