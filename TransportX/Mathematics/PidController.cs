using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Mathematics
{
    public class PidController
    {
        public PidGains K { get; set; } = new(0, 0, 0);

        public float Min { get; }
        public float Max { get; }

        private float I = 0;
        private float OldError = 0;

        public PidController(float min = float.MinValue, float max = float.MaxValue)
        {
            Min = min;
            Max = max;
        }

        public float Next(float error, TimeSpan elapsed)
        {
            float dt = (float)elapsed.TotalSeconds;

            float p = K.P * error;
            I = float.Max(Min, float.Min(I + K.I * error * dt, Max));
            float d = dt < 1e-3f ? 0 : K.D * (error - OldError) / dt;

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
