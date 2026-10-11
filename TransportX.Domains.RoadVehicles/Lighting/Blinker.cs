using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Communication;

namespace TransportX.Domains.RoadVehicles.Lighting
{
    public class Blinker
    {
        private TimeSpan ElapsedTime = TimeSpan.Zero;
        private bool WasPhaseActive = false;
        private BlinkerLeverDirection OldLeverDirection = BlinkerLeverDirection.None;

        public TimeSpan Period
        {
            get;
            set => field = TimeSpan.Zero < value ? value : throw new ArgumentOutOfRangeException(nameof(Period));
        } = TimeSpan.FromSeconds(0.75);
        public float DutyCycle { get; set; } = 0.5f;

        public IBlinkerAutoCancel AutoCancel { get; set; } = IBlinkerAutoCancel.DoNothing;

        public Signal<int> LeverSignal { get; set; } = new(0);
        public Signal<bool> HazardSignal { get; set; } = new(false);
        public Signal<int> RelayOnCountSignal { get; set; } = new(0);
        public Signal<int> RelayOffCountSignal { get; set; } = new(0);

        public BlinkerSide Left { get; } = new();
        public BlinkerSide Right { get; } = new();

        public Blinker()
        {
        }

        public void Tick(TimeSpan elapsed)
        {
            BlinkerLeverDirection leverDirection = (BlinkerLeverDirection)LeverSignal.Value;
            if (leverDirection != OldLeverDirection)
            {
                OldLeverDirection = leverDirection;
                AutoCancel.Reset();
            }

            if (leverDirection != BlinkerLeverDirection.None && AutoCancel.ShouldCancel(leverDirection, elapsed))
            {
                LeverSignal.Value = (int)(leverDirection = BlinkerLeverDirection.None);
                OldLeverDirection = BlinkerLeverDirection.None;
                AutoCancel.Reset();
            }

            bool isHazard = HazardSignal.Value;
            bool isLeftRequested = isHazard || leverDirection == BlinkerLeverDirection.Left;
            bool isRightRequested = isHazard || leverDirection == BlinkerLeverDirection.Right;
            bool isOperating = isLeftRequested || isRightRequested;

            if (!isOperating)
            {
                if (WasPhaseActive)
                {
                    WasPhaseActive = false;
                    unchecked { RelayOffCountSignal.Value++; }
                }

                ElapsedTime = TimeSpan.Zero;
                AutoCancel.Reset();
                Left.Reset();
                Right.Reset();
                return;
            }

            ElapsedTime += elapsed;
            while (Period <= ElapsedTime)
            {
                ElapsedTime -= Period;
            }

            float phase = Period.Ticks <= 0 ? 0 : (float)ElapsedTime.Ticks / Period.Ticks;
            bool isPhaseActive = phase < DutyCycle;

            if (isPhaseActive != WasPhaseActive)
            {
                WasPhaseActive = isPhaseActive;
                unchecked
                {
                    if (isPhaseActive)
                    {
                        RelayOnCountSignal.Value++;
                    }
                    else
                    {
                        RelayOffCountSignal.Value++;
                    }
                }
            }

            BlinkerContext context = new()
            {
                IsRequested = isLeftRequested,
                Phase = phase,
                DutyCycle = DutyCycle,
            };
            Left.Tick(context, elapsed);

            context = context with
            {
                IsRequested = isRightRequested,
            };
            Right.Tick(context, elapsed);
        }
    }
}
