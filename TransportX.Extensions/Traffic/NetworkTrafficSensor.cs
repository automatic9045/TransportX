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

        public ITrafficEntity? Target { get; private set; } = null;
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

        public NetworkTrafficSensor(ILaneTracker laneTracker, IWorldObject origin)
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
            if (!LaneTracker.IsEnabled || LaneTracker.Path is null) throw new InvalidOperationException();

            LanePathView pathView = new(LaneTracker.Path, LaneTracker.Heading);

            ITrafficEntity? next = null;
            float minDistance = float.MaxValue;
            float viewS = pathView.ToViewS(LaneTracker.S);
            for (int i = 0; i < LaneTracker.Path.Entities.Count; i++)
            {
                ITrafficEntity entity = LaneTracker.Path.Entities[i];
                float entityViewS = pathView.ToViewS(entity.S);
                if (viewS < entityViewS)
                {
                    float diff = entityViewS - viewS;
                    if (diff < minDistance)
                    {
                        minDistance = diff;
                        next = entity;
                    }
                }
            }

            bool isOncoming = next is not null && LaneTracker.Heading != next.Heading;

            float distance;
            float surfaceDistance;
            if (next is not null)
            {
                distance = pathView.ToViewS(next.S) - viewS;
                surfaceDistance = isOncoming ? distance : distance - next.Length;
            }
            else
            {
                distance = pathView.ToViewS(LaneTracker.Path.Length - LaneTracker.S);
                surfaceDistance = float.MaxValue;

                for (int r = 0; r < plannedRoute.Count; r++)
                {
                    if (MaxDistance < distance) break;

                    LanePathView view = plannedRoute[r];
                    IReadOnlyList<ITrafficEntity> viewEntities = view.Source.Entities;

                    ITrafficEntity? closestInPath = null;
                    float minViewS = float.MaxValue;

                    for (int e = 0; e < viewEntities.Count; e++)
                    {
                        ITrafficEntity candidate = viewEntities[e];
                        float s = view.ToViewS(candidate.S);
                        if (s < minViewS)
                        {
                            minViewS = s;
                            closestInPath = candidate;
                        }
                    }

                    if (closestInPath is null)
                    {
                        distance += view.Source.Length;
                    }
                    else
                    {
                        next = closestInPath;
                        isOncoming = (next.Heading == EntityDirection.Forward) == view.Reverse;
                        distance += minViewS;
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

        public void Draw(in TransformedDrawContext context)
        {
            if (context.Layer != RenderLayer.Traffic) throw new InvalidOperationException();

            DebugVisual.Target = Target;
            DebugVisual.IsTargetOncoming = IsTargetOncoming;
            DebugVisual.Draw(context);
        }
    }
}
