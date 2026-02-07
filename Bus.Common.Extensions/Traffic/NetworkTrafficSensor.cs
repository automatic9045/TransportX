using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery.Networks;
using Bus.Common.Traffic;

namespace Bus.Common.Extensions.Traffic
{
    public class NetworkTrafficSensor : ITrafficSensor
    {
        private readonly ILaneTracker LaneTracker;

        public ITrafficParticipant? Target { get; private set; } = null;
        public bool IsTargetOncoming { get; private set; } = false;
        public float DistanceToTarget { get; private set; } = 0;

        public NetworkTrafficSensor(ILaneTracker laneTracker)
        {
            LaneTracker = laneTracker;
        }

        public void Tick(IEnumerable<LanePathView> plannedRoute, IEnumerable<ITrafficParticipant> obstacles, TimeSpan elapsed)
        {
            if (!LaneTracker.IsEnabled || LaneTracker.Path is null) throw new InvalidOperationException();

            LanePathView pathView = new(LaneTracker.Path, LaneTracker.Heading);

            ITrafficParticipant? next = LaneTracker.Path.Participants
                .OrderBy(participant => pathView.ToViewS(participant.S))
                .FirstOrDefault(participant => pathView.ToViewS(LaneTracker.S) < pathView.ToViewS(participant.S));
            bool isOncoming = next is not null && LaneTracker.Heading != next.Heading;
            float distance;
            if (next is not null)
            {
                distance = pathView.ToViewS(next.S) - pathView.ToViewS(LaneTracker.S);
            }
            else
            {
                distance = pathView.ToViewS(LaneTracker.Path.Length - LaneTracker.S);
                foreach (LanePathView view in plannedRoute)
                {
                    next = view.Source.Participants
                        .OrderBy(participant => view.ToViewS(participant.S))
                        .FirstOrDefault();

                    if (next is not null)
                    {
                        isOncoming = (next.Heading == ParticipantDirection.Forward) == view.Reverse;
                        distance += view.ToViewS(next.S);
                        break;
                    }
                    else
                    {
                        distance += view.Source.Length;
                    }
                }
            }

            if (next is null)
            {
                Target = null;
                IsTargetOncoming = false;
                DistanceToTarget = float.MaxValue;
            }
            else
            {
                Target = next;
                IsTargetOncoming = isOncoming;
                DistanceToTarget = distance;
            }
        }
    }
}
