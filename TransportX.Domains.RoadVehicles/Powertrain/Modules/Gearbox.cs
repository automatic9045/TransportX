using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Domains.RoadVehicles.Physics;
using TransportX.Domains.RoadVehicles.Powertrain.Constraints;

namespace TransportX.Domains.RoadVehicles.Powertrain.Modules
{
    public class Gearbox : IModule
    {
        public required IReadOnlyList<float> GearRatios { get; init; }
        public required IReadOnlyList<float> ReverseGearRatios { get; init; }

        public Shaft Input { get; }
        public Shaft Output { get; }
        public TransmissionConstraint Constraint { get; }

        public IReadOnlyList<Shaft> InputShafts { get; }
        public IReadOnlyList<Shaft> OutputShafts { get; }
        public IReadOnlyList<IConstraint> Constraints { get; }

        public int Gear
        {
            get => field;
            set => field = int.Clamp(value, -ReverseGearRatios.Count, GearRatios.Count);
        }

        public float Ratio => GetGearRatio(Gear);
        public int MinGear => ReverseGearRatios.Count;
        public int MaxGear => GearRatios.Count;

        public Gearbox(Shaft input, Shaft output)
        {
            Input = input;
            Output = output;
            Constraint = new TransmissionConstraint(Input, Output);

            InputShafts = [Input];
            OutputShafts = [Output];
            Constraints = [Constraint];
        }

        public float GetGearRatio(int gear)
        {
            return gear switch
            {
                int _ when gear < -ReverseGearRatios.Count => throw new ArgumentOutOfRangeException(nameof(gear)),
                int _ when gear < 0 => ReverseGearRatios[-gear - 1],
                0 => 0,
                int _ when GearRatios.Count < gear => throw new ArgumentOutOfRangeException(nameof(gear)),
                _ => GearRatios[gear - 1],
            };
        }

        public virtual void Tick(TimeSpan elapsed)
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
