using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Sample.Vehicles.Interfaces;
using Bus.Sample.Vehicles.Powertrain.Constraints;
using Bus.Sample.Vehicles.Powertrain.Physics;

namespace Bus.Sample.Vehicles.Powertrain.Modules
{
    internal abstract class ClutchBase : IModule
    {
        protected readonly IAxis Axis;

        protected readonly Shaft Input;
        protected readonly Shaft Output;

        protected readonly ClutchConstraint Constraint;

        public IEnumerable<IConstraint> Constraints { get; }
        public float Engagement => Axis.Rate;

        protected ClutchBase(IAxis axis, Shaft input, Shaft output)
        {
            Axis = axis;

            Input = input;
            Output = output;

            Constraint = new ClutchConstraint(Input, Output);
            Constraints = [Constraint];
        }

        public abstract void Tick(TimeSpan elapsed);

        public void PropagateTorque()
        {
            Constraint.PropagateTorque(Input);
        }
    }
}
