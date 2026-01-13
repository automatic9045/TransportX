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
    internal class Clutch : IModule
    {
        private static readonly float FrictionCoefficient = 900;


        private readonly Pedal ClutchPedal;

        private readonly Shaft Input;
        private readonly Shaft Output;

        private readonly ClutchConstraint Constraint;

        public IEnumerable<IConstraint> Constraints { get; }
        public float Engagement => ClutchPedal.Rate;

        public Clutch(Pedal clutchPedal, Shaft input, Shaft output)
        {
            ClutchPedal = clutchPedal;

            Input = input;
            Output = output;

            Constraint = new ClutchConstraint(Input, Output);
            Constraints = [Constraint];
        }

        public void Tick(TimeSpan elapsed)
        {
            if (Engagement < 1e-3f)
            {
                Constraint.IsEnabled = false;
                Output.Torque = 0;
            }
            else if (1 - 1e-3f < Engagement)
            {
                Constraint.IsEnabled = true;
            }
            else
            {
                Constraint.IsEnabled = false;

                float delta = Input.AngularVelocity - Output.AngularVelocity;
                float torque = float.Sign(delta) * FrictionCoefficient * Engagement;
                Input.ApplyTorque(-torque, elapsed);
                Output.ApplyTorque(torque, elapsed);
            }
        }

        public void PropagateTorque()
        {
            Constraint.PropagateTorque(Input);
        }
    }
}
