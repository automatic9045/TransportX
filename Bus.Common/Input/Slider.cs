using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Input
{
    public abstract class Slider : IDisposable
    {
        public double Min { get; set; }
        public double Max { get; set; }
        public double Rate { get; set; } = 0;

        public Slider(double min, double max)
        {
            Min = min;
            Max = max;
        }

        public abstract void Dispose();

        public abstract void Tick(TimeSpan elapsed);
    }
}
