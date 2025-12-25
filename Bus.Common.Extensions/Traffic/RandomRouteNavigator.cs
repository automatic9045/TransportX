using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery.Networks;

namespace Bus.Common.Extensions.Traffic
{
    public class RandomRouteNavigator : IRouteNavigator
    {
        private readonly Queue<LanePathView> PlannedRouteKey = new();

        public IReadOnlyCollection<LanePathView> PlannedRoute => PlannedRouteKey;
        public float PlannedLength => PlannedRoute.Sum(p => p.Source.Length);

        public RandomRouteNavigator()
        {
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
                    nextPin.SourcePaths.Select(p => new LanePathView(p, false)),
                    nextPin.DestPaths.Select(p => new LanePathView(p, true))
                ).ToArray();
                if (candidates.Count == 0) break;

                LanePathView next = candidates[Random.Shared.Next(candidates.Count)];

                PlannedRouteKey.Enqueue(next);

                tail = next;
                plannedLength += next.Source.Length;
            }
        }
    }
}
