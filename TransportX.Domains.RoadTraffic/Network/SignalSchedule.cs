using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Domains.RoadTraffic.Network
{
    public class SignalSchedule
    {
        public static readonly SignalSchedule Empty = new(new Dictionary<TimeSpan, Plan>());


        private readonly TimeSpan[] StartTimes;
        private readonly Plan[] Plans;

        private int Index = 0;
        
        private bool IsFirstTick = true;
        private int ApplyStartIndex = 0;
        private int ApplyLength = 0;

        public Plan CurrentPlan => Plans.Length == 0 ? Plan.Empty : Plans[Index];
        public ReadOnlySpan<Plan> PlansToApply => new(Plans, ApplyStartIndex, ApplyLength);

        public SignalSchedule(IReadOnlyDictionary<TimeSpan, Plan> plans)
        {
            IEnumerable<KeyValuePair<TimeSpan, Plan>> sorted = plans.OrderBy(x => x.Key);
            StartTimes = sorted.Select(x => x.Key).ToArray();
            Plans = sorted.Select(x => x.Value).ToArray();
        }

        public void Tick(DateTime now)
        {
            if (Plans.Length == 0) return;

            int nextIndex = Plans.Length - 1;
            for (int i = 0; i < StartTimes.Length; i++)
            {
                if (StartTimes[i] <= now.TimeOfDay)
                {
                    nextIndex = i;
                }
                else
                {
                    break;
                }
            }

            if (IsFirstTick || nextIndex < Index)
            {
                ApplyStartIndex = 0;
                ApplyLength = nextIndex + 1;
                IsFirstTick = false;
            }
            else
            {
                ApplyStartIndex = Index;
                ApplyLength = nextIndex - Index + 1;
            }

            Index = nextIndex;
        }


        public class Plan
        {
            public static readonly Plan Empty = new(TimeSpan.Zero, []);


            public TimeSpan Offset { get; }
            public IReadOnlyList<Step> Steps { get; }
            public TimeSpan CycleLength { get; }

            public Plan(TimeSpan offset, IReadOnlyList<Step> steps)
            {
                Offset = offset;
                Steps = steps;

                TimeSpan cycleLength = TimeSpan.Zero;
                for (int i = 0; i < Steps.Count; i++) cycleLength += Steps[i].Duration;
                CycleLength = cycleLength;
            }
        }

        public class Step
        {
            public TimeSpan Duration { get; }
            public IReadOnlyDictionary<string, SignalColor> Signals { get; }

            public Step(TimeSpan duration, IReadOnlyDictionary<string, SignalColor> signals)
            {
                Duration = duration;
                Signals = signals;
            }
        }
    }
}
