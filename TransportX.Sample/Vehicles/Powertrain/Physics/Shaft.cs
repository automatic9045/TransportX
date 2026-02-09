using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Sample.Vehicles.Powertrain.Physics
{
    internal class Shaft
    {
        private static readonly float ToRpm = 1 / (2 * float.Pi) * 60;


        public float Inertia { get; }
        public float InertiaInverse { get; }

        public float AngularVelocity { get; set; } = 0;
        public float Rpm
        {
            get => AngularVelocity * ToRpm;
            set => AngularVelocity = value / ToRpm;
        }

        public float Torque { get; set; } = 0;

        public Shaft(float inertia)
        {
            Inertia = inertia;
            InertiaInverse = 1 / Inertia;
        }

        public void ApplyTorque(float torque, TimeSpan elapsed)
        {
            Torque = torque;
            ApplyImpulse(torque * (float)elapsed.TotalSeconds);
        }

        public void ApplyImpulse(float impulse)
        {
            AngularVelocity += InertiaInverse * impulse;
        }
    }
}
