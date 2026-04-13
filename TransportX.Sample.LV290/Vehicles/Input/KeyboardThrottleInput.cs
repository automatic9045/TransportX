using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Input;

namespace TransportX.Sample.LV290.Vehicles.Input
{
    internal class KeyboardThrottleInput : SliderByKey
    {
        private readonly KeyObserver BoostKey;

        public KeyboardThrottleInput(KeyObserver key, KeyObserver boostKey) : base(key, 1, 1.2f)
        {
            BoostKey = boostKey;
        }

        public override void Dispose()
        {
            base.Dispose();
            BoostKey.Dispose();
        }

        public override void Tick(TimeSpan elapsed)
        {
            float rate = Rate + (Source.IsPressed ? IncreaseSpeed : -DecreaseSpeed) * (float)elapsed.TotalSeconds;
            Rate = float.Max(Min, float.Min(rate, BoostKey.IsPressed ? 1 : 0.75f));
        }
    }
}
