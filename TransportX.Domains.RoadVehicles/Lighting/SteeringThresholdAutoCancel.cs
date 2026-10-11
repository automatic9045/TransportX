using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Communication;

namespace TransportX.Domains.RoadVehicles.Lighting
{
    public class SteeringThresholdAutoCancel : IBlinkerAutoCancel
    {
        private readonly Signal<float> SteeringSignal;

        private bool HasSteered = false;
        private BlinkerLeverDirection OldLeverDirection = BlinkerLeverDirection.None;

        public float EngageThreshold { get; init; } = 0.25f;
        public float DisengageThreshold { get; init; } = 0.08f;

        public SteeringThresholdAutoCancel(Signal<float> steeringSignal)
        {
            SteeringSignal = steeringSignal;
        }

        public bool ShouldCancel(BlinkerLeverDirection leverDirection, TimeSpan elapsed)
        {
            if (leverDirection != OldLeverDirection)
            {
                OldLeverDirection = leverDirection;
                HasSteered = false;
            }

            float steering = SteeringSignal.Value;
            switch (OldLeverDirection)
            {
                case BlinkerLeverDirection.Left:
                    if (steering < -EngageThreshold)
                    {
                        HasSteered = true;
                    }
                    else if (HasSteered && -DisengageThreshold < steering)
                    {
                        Reset();
                        return true;
                    }
                    break;

                case BlinkerLeverDirection.Right:
                    if (EngageThreshold < steering)
                    {
                        HasSteered = true;
                    }
                    else if (HasSteered && steering < DisengageThreshold)
                    {
                        Reset();
                        return true;
                    }
                    break;
            }

            return false;
        }

        public void Reset()
        {
            HasSteered = false;
            OldLeverDirection = BlinkerLeverDirection.None;
        }
    }
}
