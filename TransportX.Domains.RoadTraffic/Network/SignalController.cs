using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Domains.RoadTraffic.Network
{
    public class SignalController : ISignalController
    {
        public static readonly SignalController Empty = new(SignalSchedule.Empty);


        private readonly SignalSchedule Schedule;

        private readonly Dictionary<string, SignalColor> SignalsKey = [];
        public IReadOnlyDictionary<string, SignalColor> Signals => SignalsKey;

        public SignalController(SignalSchedule schedule)
        {
            Schedule = schedule;
        }

        public void Tick(DateTime now)
        {
            Schedule.Tick(now);

            ReadOnlySpan<SignalSchedule.Plan> plansToApply = Schedule.PlansToApply;
            if (plansToApply.Length == 0) return;

            for (int p = 0; p < plansToApply.Length - 1; p++)
            {
                SignalSchedule.Plan pastPlan = plansToApply[p];
                for (int s = 0; s < pastPlan.Steps.Count; s++)
                {
                    foreach ((string groupKey, SignalColor signal) in pastPlan.Steps[s].Signals)
                    {
                        SignalsKey[groupKey] = signal;
                    }
                }
            }

            SignalSchedule.Plan currentPlan = plansToApply[^1];
            if (currentPlan.CycleLength <= TimeSpan.Zero) return;

            TimeSpan cycleTime = TimeSpan.FromTicks((now.TimeOfDay - currentPlan.Offset).Ticks % currentPlan.CycleLength.Ticks);
            if (cycleTime < TimeSpan.Zero)
            {
                cycleTime += currentPlan.CycleLength;
            }

            TimeSpan totalTime = TimeSpan.Zero;
            for (int i = 0; i < currentPlan.Steps.Count; i++)
            {
                SignalSchedule.Step step = currentPlan.Steps[i];

                foreach ((string groupKey, SignalColor signal) in step.Signals)
                {
                    SignalsKey[groupKey] = signal;
                }

                totalTime += step.Duration;
                if (cycleTime < totalTime) break;
            }
        }

        public SignalColor GetSignal(string groupKey)
        {
            return SignalsKey.TryGetValue(groupKey, out SignalColor signal) ? signal : SignalColor.Off;
        }
    }
}
