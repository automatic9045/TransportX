using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Network;
using TransportX.Rendering;
using TransportX.Spatial;
using TransportX.Traffic;

using TransportX.Extensions.Traffic;

using TransportX.Domains.RoadTraffic.Network;

namespace TransportX.Domains.RoadTraffic.Traffic.Sensors
{
    public class PriorityTrafficSensor : ITrafficSensor
    {
        private readonly record struct SearchNode(LanePathSegmentView SegmentView, float DistanceToStart);


        private const float YieldSearchDistance = 50;


        private readonly Queue<SearchNode> SearchQueue = new();
        private readonly HashSet<ILanePath> VisitedPaths = [];

        private readonly ILaneTracker LaneTracker;
        private readonly TrafficSensorDebugVisual DebugVisual;

        public float MaxDistance { get; set; } = float.MaxValue;

        public ITrafficParticipant? Target { get; private set; } = null;
        public bool IsTargetOncoming => false;
        public float DistanceToTarget { get; private set; } = 0;
        public float StopMargin { get; init; } = 0.75f;

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

        public PriorityTrafficSensor(ILaneTracker laneTracker, ILocatable location)
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
            if (LaneTracker.Path is null) throw new InvalidOperationException();

            float totalLength = LaneTracker.Path.Length - new LanePathView(LaneTracker.Path, LaneTracker.Heading).ToViewS(LaneTracker.S);
            ITrafficParticipant? next = null;

            foreach (LanePathView view in plannedRoute)
            {
                if (MaxDistance < totalLength) break;

                if (view.Source.Components.TryGet<YieldComponent>(out YieldComponent? component))
                {
                    if (HasApproachingVehicle(component.PrioritySegments, YieldSearchDistance))
                    {
                        next = new PriorityParticipant(view);
                        break;
                    }
                }

                if (next is not null) break;
                totalLength += view.Source.Length;
            }

            if (next is null)
            {
                Target = null;
                DistanceToTarget = float.MaxValue;
            }
            else
            {
                Target = next;
                DistanceToTarget = totalLength;
            }


            bool HasApproachingVehicle(IReadOnlyList<LanePathSegment> startSegments, float maxDistance)
            {
                SearchQueue.Clear();
                VisitedPaths.Clear();

                for (int i = 0; i < startSegments.Count; i++)
                {
                    LanePathSegment segment = startSegments[i];

                    if (segment.Path.Directions.HasFlag(FlowDirections.Out))
                    {
                        LanePathView view = new(segment.Path, false);
                        SearchQueue.Enqueue(new SearchNode(new LanePathSegmentView(view, view.ToViewS(segment.MinS), view.ToViewS(segment.MaxS)), 0));
                    }

                    if (segment.Path.Directions.HasFlag(FlowDirections.In))
                    {
                        LanePathView view = new(segment.Path, true);
                        SearchQueue.Enqueue(new SearchNode(new LanePathSegmentView(view, view.ToViewS(segment.MaxS), view.ToViewS(segment.MinS)), 0));
                    }
                }

                while (0 < SearchQueue.Count)
                {
                    SearchNode node = SearchQueue.Dequeue();
                    LanePathSegmentView currentSegmentView = node.SegmentView;
                    LanePathView currentView = currentSegmentView.Path;

                    if (!VisitedPaths.Add(currentView.Source)) continue;

                    IReadOnlyList<ITrafficParticipant> participants = currentView.Source.Participants;
                    for (int i = 0; i < participants.Count; i++)
                    {
                        ITrafficParticipant participant = participants[i];
                        ParticipantDirection expectedHeading = currentView.Reverse ? ParticipantDirection.Backward : ParticipantDirection.Forward;

                        if (participant.Heading == expectedHeading)
                        {
                            float viewS = currentView.ToViewS(participant.S);

                            if (currentSegmentView.MaxViewS < viewS) continue; // 優先区間通過済

                            if (currentSegmentView.MinViewS <= viewS) return true; // 優先区間内

                            float vehicleDistance = node.DistanceToStart + (currentSegmentView.MinViewS - viewS);
                            if (vehicleDistance <= maxDistance) // 優先区間接近中
                            {
                                float speed = float.Abs(participant.SVelocity);
                                if (1e-3f < speed && (vehicleDistance < 5 || vehicleDistance / speed < 4))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    float nextDistance = node.DistanceToStart + currentSegmentView.MinViewS;
                    if (maxDistance < nextDistance) continue;

                    LanePin entryPin = currentView.From;
                    LanePin? prevPin = entryPin.ConnectedPin;

                    if (prevPin is not null)
                    {
                        for (int i = 0; i < prevPin.DestPaths.Count; i++)
                        {
                            ILanePath path = prevPin.DestPaths[i];
                            if (path.Directions.HasFlag(FlowDirections.Out))
                            {
                                LanePathSegmentView segmentView = new(new LanePathView(path, false), path.Length, path.Length);
                                SearchQueue.Enqueue(new SearchNode(segmentView, nextDistance));
                            }
                        }

                        for (int i = 0; i < prevPin.SourcePaths.Count; i++)
                        {
                            ILanePath path = prevPin.SourcePaths[i];
                            if (path.Directions.HasFlag(FlowDirections.In))
                            {
                                LanePathSegmentView segmentView = new(new LanePathView(path, true), path.Length, path.Length);
                                SearchQueue.Enqueue(new SearchNode(segmentView, nextDistance));
                            }
                        }
                    }
                }

                return false;
            }
        }

        public void Draw(in LocatedDrawContext context)
        {
            if (context.Pass != RenderPass.Traffic) throw new InvalidOperationException();

            DebugVisual.Target = Target;
            DebugVisual.IsTargetOncoming = IsTargetOncoming;
            DebugVisual.Draw(context);
        }


        private readonly struct PriorityParticipant : ITrafficParticipant
        {
            private readonly LanePathView Target;

            public readonly int PlateX => Target.Source.Owner.PlateX;
            public readonly int PlateZ => Target.Source.Owner.PlateZ;
            public readonly Pose Pose { get; }
            public readonly Vector3 Velocity => Vector3.Zero;

            public readonly float Width => 0;
            public readonly float Height => 0;
            public readonly float Length => 0;

            public readonly bool IsEnabled => true;
            public readonly ILanePath? Path => Target.Source;
            public readonly ParticipantDirection Heading { get; }
            public readonly float S { get; }
            public readonly float SVelocity => 0;

            public event Action<PlateOffset>? Moved
            {
                add => throw new NotSupportedException();
                remove => throw new NotSupportedException();
            }

            public PriorityParticipant(in LanePathView target)
            {
                Target = target;
                Pose = Target.GetPose(0);
                Heading = Target.Reverse ? ParticipantDirection.Backward : ParticipantDirection.Forward;
                S = Target.FromViewS(0);
            }

            public readonly bool Spawn(ILanePath path, ParticipantDirection heading, float s)
            {
                throw new NotSupportedException();
            }
        }
    }
}
