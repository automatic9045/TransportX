using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Mathematics
{
    public class Debouncer
    {
        private bool TargetValue;
        private TimeSpan ElapsedSinceChange;

        public TimeSpan Delay { get; set; }
        public bool Value { get; private set; }

        public Debouncer(TimeSpan delay, bool initialValue = false)
        {
            Delay = delay;
            Value = initialValue;
            TargetValue = initialValue;
            ElapsedSinceChange = TimeSpan.Zero;
        }

        public bool Next(bool input, TimeSpan elapsed)
        {
            if (input != TargetValue)
            {
                TargetValue = input;
                ElapsedSinceChange = TimeSpan.Zero;
            }

            if (input != Value)
            {
                ElapsedSinceChange += elapsed;

                if (Delay <= ElapsedSinceChange)
                {
                    Value = input;
                }
            }

            return Value;
        }

        public void Reset(bool value = false)
        {
            Value = value;
            TargetValue = value;
            ElapsedSinceChange = TimeSpan.Zero;
        }
    }
}
