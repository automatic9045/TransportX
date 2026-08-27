using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Mathematics
{
    public class Vector3LowPassFilter
    {
        public float TimeConstant { get; set; }
        public Vector3 Value { get; private set; }

        public Vector3LowPassFilter(float timeConstant, Vector3 initialValue = default)
        {
            TimeConstant = timeConstant;
            Value = initialValue;
        }

        public Vector3 Next(Vector3 input, TimeSpan elapsed)
        {
            float dt = (float)elapsed.TotalSeconds;
            if (dt <= 0) return Value;

            float alpha = dt / (TimeConstant + dt);

            Value = Value + alpha * (input - Value);
            return Value;
        }

        public void Reset(Vector3 value = default)
        {
            Value = value;
        }
    }
}
