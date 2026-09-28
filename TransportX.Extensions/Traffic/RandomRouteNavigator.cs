using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Collections;
using TransportX.Network;
using TransportX.Traffic;

namespace TransportX.Extensions.Traffic
{
    public class RandomRouteNavigator : IRouteNavigator
    {
        private readonly RingBuffer<LanePathView> PlannedRouteKey = new(16);

        private readonly List<LanePathView> CandidateBuffer = new(8);
        private readonly List<float> WeightBuffer = new(8);

        public IReadOnlyList<LanePathView> PlannedRoute => PlannedRouteKey;
        public float PlannedLength { get; private set; } = 0;

        public RandomRouteNavigator()
        {
        }

        public void Reset()
        {
            PlannedRouteKey.Clear();
            PlannedLength = 0;
        }

        public bool TryPop(out LanePathView pathView)
        {
            if (!PlannedRouteKey.TryPop(out pathView)) return false;

            PlannedLength -= pathView.Source.Length;
            if (PlannedLength < 0) PlannedLength = 0;
            return true;
        }

        public void Update(LanePathView currentPath, float planLength)
        {
            LanePathView tail = 0 < PlannedRouteKey.Count ? PlannedRouteKey[^1] : currentPath;

            while (PlannedLength < planLength)
            {
                LanePin? nextPin = tail.To.ConnectedPin;
                if (nextPin is null) break;

                CandidateBuffer.Clear();

                IReadOnlyList<ILanePath> sourcePaths = nextPin.SourcePaths;
                for (int i = 0; i < sourcePaths.Count; i++)
                {
                    ILanePath path = sourcePaths[i];
                    if (path.Directions.HasFlag(FlowDirections.Out))
                    {
                        CandidateBuffer.Add(new LanePathView(path, false));
                    }
                }

                IReadOnlyList<ILanePath> destPaths = nextPin.DestPaths;
                for (int i = 0; i < destPaths.Count; i++)
                {
                    ILanePath path = destPaths[i];
                    if (path.Directions.HasFlag(FlowDirections.In))
                    {
                        CandidateBuffer.Add(new LanePathView(path, true));
                    }
                }

                if (CandidateBuffer.Count == 0) break;

                LanePathView next;
                if (CandidateBuffer.Count == 1)
                {
                    next = CandidateBuffer[0];
                }
                else
                {
                    WeightBuffer.Clear();
                    float totalWeight = 0;

                    for (int i = 0; i < CandidateBuffer.Count; i++)
                    {
                        float weight = 1;
                        if (CandidateBuffer[i].Source.Components.TryGet<TrafficDensityComponent>(out TrafficDensityComponent? density))
                        {
                            weight = float.Max(0, density.Factor);
                        }
                        WeightBuffer.Add(weight);
                        totalWeight += weight;
                    }

                    if (totalWeight <= 0)
                    {
                        next = CandidateBuffer[Random.Shared.Next(CandidateBuffer.Count)];
                    }
                    else
                    {
                        float randomValue = Random.Shared.NextSingle() * totalWeight;
                        float cumulativeWeight = 0;
                        int selectedIndex = CandidateBuffer.Count - 1;

                        for (int i = 0; i < CandidateBuffer.Count; i++)
                        {
                            cumulativeWeight += WeightBuffer[i];
                            if (randomValue < cumulativeWeight)
                            {
                                selectedIndex = i;
                                break;
                            }
                        }
                        next = CandidateBuffer[selectedIndex];
                    }
                }

                PlannedRouteKey.Add(next);
                PlannedLength += next.Source.Length;
                tail = next;
            }
        }
    }
}
