using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Input
{
    public class SliderByKey : Slider
    {
        public KeyObserver Source { get; set; }
        public double IncreaseSpeed { get; set; }
        public double DecreaseSpeed { get; set; }
        public bool Reverse { get; set; }

        public SliderByKey(KeyObserver source, double increaseSpeed, double decreaseSpeed, double min = 0, double max = 1, bool reverse = false) : base(min, max)
        {
            Source = source;
            IncreaseSpeed = increaseSpeed;
            DecreaseSpeed = decreaseSpeed;
            Reverse = reverse;
        }

        public override void Dispose()
        {
            Source.Dispose();
        }

        public override void Tick(TimeSpan elapsed)
        {
            double rate = Rate + (Reverse ^ Source.IsPressed ? IncreaseSpeed : -DecreaseSpeed) * elapsed.TotalSeconds;
            Rate = double.Max(Min, double.Min(rate, Max));
        }
    }
}
