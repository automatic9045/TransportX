using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Sample.Vehicles.Interfaces;
using TransportX.Sample.Vehicles.Powertrain.Physics;

namespace TransportX.Sample.Vehicles.Powertrain.Modules
{
    internal class FrictionClutch : ClutchBase
    {
        private static readonly float FrictionCoefficient = 900;


        public FrictionClutch(IAxis axis, Shaft input, Shaft output) : base(axis, input, output)
        {
        }

        public override void Tick(TimeSpan elapsed)
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
    }
}
