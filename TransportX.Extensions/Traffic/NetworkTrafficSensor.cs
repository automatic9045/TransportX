using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Network;
using TransportX.Rendering;
using TransportX.Traffic;

namespace TransportX.Extensions.Traffic
{
    public class NetworkTrafficSensor : ITrafficSensor
    {
        private readonly ILaneTracker LaneTracker;
        private readonly TrafficSensorDebugVisual DebugVisual;

        public float MaxDistance { get; set; } = float.MaxValue;

        public ITrafficParticipant? Target { get; private set; } = null;
        public bool IsTargetOncoming { get; private set; } = false;
        public float DistanceToTarget { get; private set; } = 0;
        public float StopMargin { get; init; } = 2;

        public Vector4 DebugColor
        {
            get => DebugVisual.DebugColor;
            set => DebugVisual.DebugColor = value;
        }
        public string? DebugName
        {
            get => DebugVisual.DebugName;
            set => DebugVisual.DebugName = value;
        }

        public NetworkTrafficSensor(ILaneTracker laneTracker, ILocatable location)
        {
            LaneTracker = laneTracker;
            DebugVisual = new TrafficSensorDebugVisual(location);
        }

        public void Dispose()
        {
            DebugVisual?.Dispose();
        }

        public void Tick(IReadOnlyCollection<LanePathView> plannedRoute, IEnumerable<ITrafficParticipant> obstacles, TimeSpan elapsed)
        {
            if (!LaneTracker.IsEnabled || LaneTracker.Path is null) throw new InvalidOperationException();

            LanePathView pathView = new(LaneTracker.Path, LaneTracker.Heading);

            ITrafficParticipant? next = LaneTracker.Path.Participants
                .OrderBy(participant => pathView.ToViewS(participant.S))
                .FirstOrDefault(participant => pathView.ToViewS(LaneTracker.S) < pathView.ToViewS(participant.S));
            bool isOncoming = next is not null && LaneTracker.Heading != next.Heading;

            float distance;
            float surfaceDistance;
            if (next is not null)
            {
                distance = pathView.ToViewS(next.S) - pathView.ToViewS(LaneTracker.S);
                surfaceDistance = isOncoming ? distance : distance - next.Length;
            }
            else
            {
                distance = pathView.ToViewS(LaneTracker.Path.Length - LaneTracker.S);
                surfaceDistance = float.MaxValue;

                foreach (LanePathView view in plannedRoute)
                {
                    if (MaxDistance < distance) break;

                    next = view.Source.Participants
                        .OrderBy(participant => view.ToViewS(participant.S))
                        .FirstOrDefault();

                    if (next is null)
                    {
                        distance += view.Source.Length;
                    }
                    else
                    {
                        isOncoming = (next.Heading == ParticipantDirection.Forward) == view.Reverse;
                        distance += view.ToViewS(next.S);
                        surfaceDistance = isOncoming ? distance : distance - next.Length;

                        if (MaxDistance < surfaceDistance) next = null;
                        break;
                    }
                }
            }

            if (next is null || MaxDistance < surfaceDistance)
            {
                Target = null;
                IsTargetOncoming = false;
                DistanceToTarget = float.MaxValue;
            }
            else
            {
                Target = next;
                IsTargetOncoming = isOncoming;
                DistanceToTarget = surfaceDistance;
            }
        }

        public void Draw(in LocatedDrawContext context)
        {
            if (context.Pass != RenderPass.Traffic) throw new InvalidOperationException();

            DebugVisual.Target = Target;
            DebugVisual.IsTargetOncoming = IsTargetOncoming;
            DebugVisual.Draw(context);
        }
    }
}
