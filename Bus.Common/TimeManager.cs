using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common
{
    public class TimeManager : ITimeManager
    {
        private const int FrameSpan = 60;


        private readonly Stopwatch Stopwatch = new Stopwatch();
        private readonly Queue<TimeSpan> Elapsed = new Queue<TimeSpan>(FrameSpan);
        private TimeSpan LastElapsed = TimeSpan.Zero;

        public TimeSpan LimitDeltaTime { get; set; } = TimeSpan.FromSeconds(1d / 6);
        public double Fps { get; private set; } = 0;
        public TimeSpan DeltaTime { get; private set; } = TimeSpan.FromTicks(1);

        public TimeManager()
        {
            Stopwatch.Start();
        }

        public void Tick()
        {
            TimeSpan elapsed = Stopwatch.Elapsed;
            TimeSpan dt = elapsed - LastElapsed;
            DeltaTime = dt < LimitDeltaTime ? dt : LimitDeltaTime;
            LastElapsed = elapsed;

            Elapsed.TryPeek(out TimeSpan firstElapsed);
            while (FrameSpan <= Elapsed.Count) firstElapsed = Elapsed.Dequeue();

            Fps = Elapsed.Count / (elapsed - firstElapsed).TotalSeconds;

            Elapsed.Enqueue(elapsed);
        }
    }
}
