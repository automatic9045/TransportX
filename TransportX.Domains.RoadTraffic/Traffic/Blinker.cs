using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Spatial;

namespace TransportX.Domains.RoadTraffic.Traffic
{
    public class Blinker
    {
        private readonly LocatedModel Model;
        private readonly TimeSpan Period;

        private TimeSpan Elapsed = TimeSpan.Zero;

        public bool IsActive { get; set; } = false;

        public Blinker(LocatedModel model, TimeSpan period)
        {
            if (period.Ticks <= 0) throw new ArgumentOutOfRangeException(nameof(period));

            Model = model;
            Period = period;
        }

        public void Tick(TimeSpan elapsed)
        {
            if (IsActive)
            {
                Elapsed += elapsed;
                while (Period <= Elapsed) Elapsed -= Period;

                Model.IsVisible = Elapsed < Period * 0.6f;
            }
            else
            {
                Elapsed = TimeSpan.Zero;
                Model.IsVisible = false;
            }
        }
    }
}
