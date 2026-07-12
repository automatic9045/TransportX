using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Mathematics
{
    public class LowPassFilter
    {
        public float TimeConstant { get; set; }
        public float Value { get; private set; }

        public LowPassFilter(float timeConstant, float initialValue = 0)
        {
            TimeConstant = timeConstant;
            Value = initialValue;
        }

        public float Next(float input, TimeSpan elapsed)
        {
            float dt = (float)elapsed.TotalSeconds;
            if (dt <= 0) return Value;

            float alpha = dt / (TimeConstant + dt);

            Value = Value + alpha * (input - Value);
            return Value;
        }

        public void Reset(float value = 0)
        {
            Value = value;
        }
    }
}
