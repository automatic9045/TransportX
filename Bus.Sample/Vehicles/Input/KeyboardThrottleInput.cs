using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Input;

namespace Bus.Sample.Vehicles.Input
{
    internal class KeyboardThrottleInput : SliderByKey
    {
        private readonly KeyObserver BoostKey;

        public KeyboardThrottleInput(KeyObserver key, KeyObserver boostKey) : base(key, 1, 1.2)
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
            double rate = Rate + (Source.IsPressed ? IncreaseSpeed : -DecreaseSpeed) * elapsed.TotalSeconds;
            Rate = double.Max(Min, double.Min(rate, BoostKey.IsPressed ? 1 : 0.5));
        }
    }
}
