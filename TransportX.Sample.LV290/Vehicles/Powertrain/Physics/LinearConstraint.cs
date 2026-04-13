using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Sample.LV290.Vehicles.Powertrain.Physics
{
    internal class LinearConstraint : IConstraint
    {
        public bool IsEnabled { get; set; } = true;

        private readonly Dictionary<Shaft, float> CoefficientsKey;
        protected IReadOnlyDictionary<Shaft, float> Coefficients => CoefficientsKey;
        protected float EffectiveMassInverse { get; private set; } = float.NaN;

        protected LinearConstraint(IEnumerable<Shaft> ports)
        {
            CoefficientsKey = ports.ToDictionary(port => port, _ => 0f);
        }

        protected LinearConstraint(params Shaft[] ports) : this((IEnumerable<Shaft>)ports)
        {
        }

        protected void SetCoefficient(Shaft port, float value)
        {
            CoefficientsKey[port] = value;
        }

        protected void Build()
        {
            float effectiveMass = Coefficients.Sum(x => x.Key.InertiaInverse * x.Value * x.Value);
            EffectiveMassInverse = 1 / effectiveMass;
        }

        public void Solve()
        {
            if (!IsEnabled) return;

            float error = Coefficients.Sum(x => x.Value * x.Key.AngularVelocity);
            float impulse = -error * EffectiveMassInverse;

            foreach ((Shaft port, float coefficient) in Coefficients)
            {
                port.ApplyImpulse(coefficient * impulse);
            }
        }

        public void PropagateTorque(Shaft input)
        {
            if (!IsEnabled) return;
            if (!Coefficients.TryGetValue(input, out float inputCoeff) || inputCoeff == 0) throw new ArgumentException();

            float lambda = -input.Torque / inputCoeff;
            foreach ((Shaft port, float coefficient) in Coefficients)
            {
                if (port == input) continue;
                port.Torque = lambda * coefficient;
            }
        }
    }
}
