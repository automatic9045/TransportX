using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Communication;

namespace TransportX.Domains.RoadVehicles.Lighting
{
    public class CamAutoCancel : IBlinkerAutoCancel
    {
        private readonly Signal<float> SteeringSignal;

        private BlinkerLeverDirection OldLeverDirection = BlinkerLeverDirection.None;
        private float EngagedCamPosition = float.NaN;

        public IReadOnlyList<float> CamPositions { get; init; } = [0.25f, 0.5f, 0.75f];
        public float Margin { get; init; } = 0.02f;

        public CamAutoCancel(Signal<float> steeringSignal)
        {
            SteeringSignal = steeringSignal;
        }

        public CamAutoCancel(Signal<float> steeringSignal, IReadOnlyList<float> camPositions)
        {
            SteeringSignal = steeringSignal;

            if (camPositions.Any(x => x <= 0)) throw new ArgumentOutOfRangeException(nameof(camPositions));
            CamPositions = camPositions;
        }

        public bool ShouldCancel(BlinkerLeverDirection leverDirection, TimeSpan elapsed)
        {
            if (leverDirection != OldLeverDirection)
            {
                OldLeverDirection = leverDirection;
                EngagedCamPosition = float.NaN;
            }

            float steering = SteeringSignal.Value;
            switch (leverDirection)
            {
                case BlinkerLeverDirection.Left:
                    for (int i = 0; i < CamPositions.Count; i++)
                    {
                        float camPosition = -CamPositions[i];
                        if (steering < camPosition - Margin)
                        {
                            if (float.IsNaN(EngagedCamPosition) || camPosition < EngagedCamPosition)
                            {
                                EngagedCamPosition = camPosition;
                            }
                        }
                    }

                    if (!float.IsNaN(EngagedCamPosition) && EngagedCamPosition < steering)
                    {
                        Reset();
                        return true;
                    }
                    break;

                case BlinkerLeverDirection.Right:
                    for (int i = 0; i < CamPositions.Count; i++)
                    {
                        float camPosition = CamPositions[i];
                        if (camPosition + Margin < steering)
                        {
                            if (float.IsNaN(EngagedCamPosition) || EngagedCamPosition < camPosition)
                            {
                                EngagedCamPosition = camPosition;
                            }
                        }
                    }

                    if (!float.IsNaN(EngagedCamPosition) && steering < EngagedCamPosition)
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
            OldLeverDirection = BlinkerLeverDirection.None;
            EngagedCamPosition = float.NaN;
        }
    }
}
