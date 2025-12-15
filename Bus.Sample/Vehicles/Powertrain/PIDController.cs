using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Sample.Vehicles.Powertrain
{
    internal class PIDController
    {
        private readonly float Kp;
        private readonly float Ki;
        private readonly float Kd;
        private readonly float Min;
        private readonly float Max;

        private float I = 0;
        private float OldError = 0;

        public PIDController(float kp, float ki, float kd, float min = float.MinValue, float max = float.MaxValue)
        {
            Kp = kp;
            Ki = ki;
            Kd = kd;

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
