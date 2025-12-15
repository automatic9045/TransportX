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
    internal class MT : IModule
    {
        private static readonly IReadOnlyList<float> GearRatios = [6.615f, 4.095f, 2.358f, 1.531f, 1, 0.722f];
        private static readonly float ReverseGearRatio = -6.615f;


        private readonly MTShifter Shifter;

        private readonly Shaft Input;
        private readonly Shaft Output;

        private readonly TransmissionConstraint Constraint;

        public IEnumerable<IConstraint> Constraints { get; }

        public int Gear => Shifter.Gear;
        public float Ratio => GetGearRatio(Gear);

        public MT(MTShifter shifter, Shaft input, Shaft output)
        {
            Shifter = shifter;

            Input = input;
            Output = output;

            Constraint = new TransmissionConstraint(Input, Output);
            Constraints = [Constraint];
        }

        private float GetGearRatio(int gear)
        {
            return gear switch
            {
                int _ when gear < -1 => throw new ArgumentOutOfRangeException(nameof(gear)),
                -1 => ReverseGearRatio,
                0 => 0,
                int _ when GearRatios.Count < gear => throw new ArgumentOutOfRangeException(nameof(gear)),
                _ => GearRatios[gear - 1],
            };
        }

        public void Tick(TimeSpan elapsed)
        {
            Constraint.Ratio = Ratio;
            Constraint.IsEnabled = Ratio != 0;

            if (!Constraint.IsEnabled)
            {
                Output.Torque = 0;
            }
        }

        public void PropagateTorque()
        {
            Constraint.PropagateTorque(Input);
        }
    }
}
