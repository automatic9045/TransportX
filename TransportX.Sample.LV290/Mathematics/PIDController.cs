using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Sample.Mathematics
{
    internal class PIDController
    {
        public float Kp { get; private set; } = 0;
        public float Ki { get; private set; } = 0;
        public float Kd { get; private set; } = 0;
        public (float P, float I, float D) K
        {
            get => (Kp, Ki, Kd);
            set
            {
                Kp = value.P;
                Ki = value.I;
                Kd = value.D;
            }
        }

        public float Min { get; }
        public float Max { get; }

        private float I = 0;
        private float OldError = 0;

        public PIDController(float min = float.MinValue, float max = float.MaxValue)
        {
            Min = min;
            Max = max;
        }

        public float Next(float error, TimeSpan elapsed)
        {
            float dt = (float)elapsed.TotalSeconds;

            float p = Kp * error;
            I = float.Max(Min, float.Min(I + Ki * error * dt, Max));
            float d = dt < 1e-3f ? 0 : Kd * (error - OldError) / dt;

            OldError = error;
            return float.Max(Min, float.Min(p + I + d, Max));
        }

        public void Reset()
        {
            I = 0;
            OldError = 0;
        }
    }
}
