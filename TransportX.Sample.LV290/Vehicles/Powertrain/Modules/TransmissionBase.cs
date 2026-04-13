using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Sample.LV290.Vehicles.Powertrain.Constraints;
using TransportX.Sample.LV290.Vehicles.Powertrain.Physics;

namespace TransportX.Sample.LV290.Vehicles.Powertrain.Modules
{
    internal abstract class TransmissionBase : IModule
    {
        protected readonly Shaft Input;
        protected readonly Shaft Output;

        private readonly TransmissionConstraint Constraint;

        protected abstract IReadOnlyList<float> GearRatios { get; }
        protected abstract float ReverseGearRatio { get; }

        public IEnumerable<IConstraint> Constraints { get; }

        public abstract int Gear { get; }
        public float Ratio => GetGearRatio(Gear);
        public int MaxGear => GearRatios.Count;

        public virtual float MinThrottle => 0;
        public virtual float MaxThrottle => 1;

        protected TransmissionBase(Shaft input, Shaft output)
        {
            Input = input;
            Output = output;

            Constraint = new TransmissionConstraint(Input, Output);
            Constraints = [Constraint];
        }

        protected float GetGearRatio(int gear)
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
