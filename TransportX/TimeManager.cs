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


        private readonly Stopwatch Stopwatch = new Stopwatch();
        private readonly Queue<TimeSpan> Elapsed = new Queue<TimeSpan>(FrameSpan);
        private TimeSpan LastElapsed = TimeSpan.Zero;

        public TimeSpan LimitDeltaTime { get; set; } = TimeSpan.FromSeconds(1d / 6);
        public double Fps { get; private set; } = 0;
        public TimeSpan DeltaTime { get; private set; } = EpsilonTime;

        public TimeManager()
        {
        }

        public void Tick()
        {
            TimeSpan elapsed = Stopwatch.Elapsed;
            if (!Stopwatch.IsRunning)
            {
                Stopwatch.Start();
                elapsed = TimeSpan.Zero;
            }

            TimeSpan dt = elapsed - LastElapsed;
            DeltaTime = dt < EpsilonTime ? EpsilonTime : dt < LimitDeltaTime ? dt : LimitDeltaTime;
            LastElapsed = elapsed;

            Elapsed.TryPeek(out TimeSpan firstElapsed);
            while (FrameSpan <= Elapsed.Count) firstElapsed = Elapsed.Dequeue();

            Fps = Elapsed.Count / (elapsed - firstElapsed).TotalSeconds;

            Elapsed.Enqueue(elapsed);
        }
    }
}
