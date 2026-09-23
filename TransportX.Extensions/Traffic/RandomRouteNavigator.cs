using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Network;
using TransportX.Traffic;

namespace TransportX.Extensions.Traffic
{
    public class RandomRouteNavigator : IRouteNavigator
    {
        private readonly Queue<LanePathView> PlannedRouteKey = new();

        public IReadOnlyCollection<LanePathView> PlannedRoute => PlannedRouteKey;
        public float PlannedLength => PlannedRoute.Sum(p => p.Source.Length);

        public RandomRouteNavigator()
        {
        }

        public void Reset()
        {
            PlannedRouteKey.Clear();
        }

        public bool TryPop(out LanePathView pathView)
        {
            if (PlannedRoute.Count == 0)
            {
                pathView = default;
                return false;
            }
            else
            {
                pathView = PlannedRouteKey.Dequeue();
                return true;
            }
        }

        public void Update(LanePathView currentPath, float planLength)
        {
            LanePathView tail = 0 < PlannedRoute.Count ? PlannedRoute.Last() : currentPath;

            float plannedLength = PlannedLength;
            while (plannedLength < planLength)
            {
                LanePin? nextPin = tail.To.ConnectedPin;
                if (nextPin is null) break;

                IReadOnlyList<LanePathView> candidates = Enumerable.Concat(
                    nextPin.SourcePaths
                        .Where(p => p.Directions.HasFlag(FlowDirections.Out))
                        .Select(p => new LanePathView(p, false)),
                    nextPin.DestPaths
                        .Where(p => p.Directions.HasFlag(FlowDirections.In))
                        .Select(p => new LanePathView(p, true))
                ).ToArray();
                if (candidates.Count == 0) break;

                LanePathView next;
                if (candidates.Count == 1)
                {
                    next = candidates[0];
                }
                else
                {
                    float totalWeight = 0;
                    float[] weights = new float[candidates.Count];
                    for (int i = 0; i < candidates.Count; i++)
                    {
                        float weight = 1.0f;
                        if (candidates[i].Source.Components.TryGet<TrafficDensityComponent>(out TrafficDensityComponent? density))
                        {
                            weight = float.Max(0, density.Factor);
                        }
                        weights[i] = weight;
                        totalWeight += weight;
                    }

                    if (totalWeight <= 0)
                    {
                        next = candidates[Random.Shared.Next(candidates.Count)];
                    }
                    else
                    {
                        float randomValue = Random.Shared.NextSingle() * totalWeight;
                        float cumulativeWeight = 0;
                        int selectedIndex = candidates.Count - 1;

                        for (int i = 0; i < candidates.Count; i++)
                        {
                            cumulativeWeight += weights[i];
                            if (randomValue < cumulativeWeight)
                            {
                                selectedIndex = i;
                                break;
                            }
                        }

                        next = candidates[selectedIndex];
                    }
                }

                PlannedRouteKey.Enqueue(next);

                tail = next;
                plannedLength += next.Source.Length;
            }
        }
    }
}
