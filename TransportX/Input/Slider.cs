using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Input
{
    public abstract class Slider : IDisposable
    {
        public float Min { get; set; }
        public float Max { get; set; }
        public float Rate { get; set; } = 0;

        public Slider(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public abstract void Dispose();

        public abstract void Tick(TimeSpan elapsed);
    }
}
