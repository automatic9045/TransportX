using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX
{
    public class TimeManager : ITimeManager
    {
        private static readonly int FrameSpan = 60;
        private static readonly TimeSpan EpsilonTime = TimeSpan.FromSeconds(1e-3f);


        private readonly Queue<double> Elapsed = new(FrameSpan);

        public TimeSpan LimitDeltaTime { get; set; } = TimeSpan.FromSeconds(1d / 6);
        public double Frequency { get; private set; } = 0;
        public TimeSpan DeltaTime { get; private set; } = EpsilonTime;

        public DateTime Now { get; set; } = DateTime.Now;

        public TimeManager()
        {
        }

        public void Tick(TimeSpan elapsed)
        {
            DeltaTime = elapsed < EpsilonTime ? EpsilonTime : elapsed < LimitDeltaTime ? elapsed : LimitDeltaTime;
            Now += DeltaTime;

            while (FrameSpan <= Elapsed.Count) Elapsed.Dequeue();
            Frequency = Elapsed.Count / Elapsed.Sum();

            Elapsed.Enqueue(DeltaTime.TotalSeconds);
        }
    }
}
