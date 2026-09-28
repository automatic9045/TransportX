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

        private readonly PriorityEntity EntityCache = new();

        public float MaxDistance { get; set; } = float.MaxValue;

        public ITrafficEntity? Target { get; private set; } = null;
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

        public PriorityTrafficSensor(ILaneTracker laneTracker, IWorldObject origin)
        {
            LaneTracker = laneTracker;
            DebugVisual = new TrafficSensorDebugVisual(origin);
        }

        public void Dispose()
        {
            DebugVisual?.Dispose();
        }

        public void Tick(IReadOnlyList<LanePathView> plannedRoute, TimeSpan elapsed)
        {
            if (LaneTracker.Path is null) throw new InvalidOperationException();

            float totalLength = LaneTracker.Path.Length - new LanePathView(LaneTracker.Path, LaneTracker.Heading).ToViewS(LaneTracker.S);
            ITrafficEntity? next = null;

            for (int i = 0; i < plannedRoute.Count; i++)
            {
                if (MaxDistance < totalLength) break;
                LanePathView view = plannedRoute[i];

                if (view.Source.Components.TryGet<YieldComponent>(out YieldComponent? component))
                {
                    if (HasApproachingVehicle(component.PrioritySegments, YieldSearchDistance))
                    {
                        EntityCache.Update(view);
                        next = EntityCache;
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

                    IReadOnlyList<ITrafficEntity> entities = currentView.Source.Entities;
                    for (int i = 0; i < entities.Count; i++)
                    {
                        ITrafficEntity entity = entities[i];
                        EntityDirection expectedHeading = currentView.Reverse ? EntityDirection.Backward : EntityDirection.Forward;

                        if (entity.Heading == expectedHeading)
                        {
                            float viewS = currentView.ToViewS(entity.S);

                            if (currentSegmentView.MaxViewS < viewS) continue; // 優先区間通過済

                            if (currentSegmentView.MinViewS <= viewS) return true; // 優先区間内

                            float vehicleDistance = node.DistanceToStart + (currentSegmentView.MinViewS - viewS);
                            if (vehicleDistance <= maxDistance) // 優先区間接近中
                            {
                                float speed = float.Abs(entity.SVelocity);
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

        public void Draw(in TransformedDrawContext context)
        {
            if (context.Layer != RenderLayer.Traffic) throw new InvalidOperationException();

            DebugVisual.Target = Target;
            DebugVisual.IsTargetOncoming = IsTargetOncoming;
            DebugVisual.Draw(context);
        }


        private sealed class PriorityEntity : ITrafficEntity
        {
            private LanePathView Target = default;

            public WorldPose WorldPose { get; private set; } = WorldPose.Zero;
            public Vector3 Velocity => Vector3.Zero;
            public Vector3 AngularVelocity => Vector3.Zero;

            public float Width => 0;
            public float Height => 0;
            public float Length => 0;

            public bool IsEnabled => true;
            public ILanePath? Path => Target.Source;
            public EntityDirection Heading { get; private set; } = EntityDirection.Forward;
            public float S { get; private set; } = 0;
            public float SVelocity => 0;

            public event MovedEventHandler? Moved
            {
                add => throw new NotSupportedException();
                remove => throw new NotSupportedException();
            }

            public PriorityEntity()
            {
            }

            public void Update(in LanePathView target)
            {
                Target = target;
                WorldPose = Target.GetWorldPose(0);
                Heading = Target.Reverse ? EntityDirection.Backward : EntityDirection.Forward;
                S = Target.FromViewS(0);
            }

            public bool Spawn(ILanePath path, EntityDirection heading, float s)
            {
                throw new NotSupportedException();
            }
        }
    }
}
