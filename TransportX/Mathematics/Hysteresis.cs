using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Mathematics
{
    public class Hysteresis
    {
        public float ThresholdHigh { get; set; }
        public float ThresholdLow { get; set; }
        public bool Value { get; private set; }

        public Hysteresis(float thresholdHigh, float thresholdLow, bool initialValue = false)
        {
            ThresholdHigh = thresholdHigh;
            ThresholdLow = thresholdLow;
            Value = initialValue;
        }

        public bool Next(float input)
        {
            if (ThresholdHigh < input)
            {
                Value = true;
            }
            else if (input < ThresholdLow)
            {
                Value = false;
            }

            return Value;
        }

        public void Reset(bool value = false)
        {
            Value = value;
        }
    }
}
