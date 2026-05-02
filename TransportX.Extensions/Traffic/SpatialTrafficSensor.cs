using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;

using TransportX.Network;
using TransportX.Rendering;
using TransportX.Spatial;
using TransportX.Traffic;

namespace TransportX.Extensions.Traffic
{
    public class SpatialTrafficSensor : ITrafficSensor
    {
        private const float ObstacleDetectMargin = 20;


        private readonly ILaneTracker LaneTracker;
        private readonly IWorldObject Origin;
        private readonly Func<ITrafficEntity, bool> ObstacleSkipCondition;
        private readonly TrafficSensorDebugVisual DebugVisual;

        public float MaxDistance { get; set; } = float.MaxValue;

        public ITrafficEntity? Target { get; private set; } = null;
        public bool IsTargetOncoming { get; private set; } = false;
        public float DistanceToTarget { get; private set; } = 0;
        public float StopMargin { get; init; } = 1;

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

        public SpatialTrafficSensor(ILaneTracker laneTracker, IWorldObject origin, Func<ITrafficEntity, bool> obstacleSkipCondition)
        {
            LaneTracker = laneTracker;
            Origin = origin;
            ObstacleSkipCondition = obstacleSkipCondition;

            DebugVisual = new TrafficSensorDebugVisual(Origin);
        }

        public void Dispose()
        {
            DebugVisual.Dispose();
        }

        public void Tick(IReadOnlyCollection<LanePathView> plannedRoute, IEnumerable<ITrafficEntity> obstacles, TimeSpan elapsed)
        {
            if (!LaneTracker.IsEnabled || LaneTracker.Path is null) throw new InvalidOperationException();

            float minSurfaceDistance = MaxDistance;

            Pose poseInv = Pose.Inverse(Origin.WorldPose.Pose);
            ProjectedEntity nearestObstacle = default;
            float nearestObstacleDistance = float.NaN;
            foreach (ITrafficEntity obstacle in obstacles)
            {
                if (!obstacle.IsEnabled) continue;
                if (ObstacleSkipCondition(obstacle)) continue;

                ChunkOffset offset = Origin.GetChunkOffset(obstacle);
                if (1 < int.Abs(offset.DeltaX) || 1 < int.Abs(offset.DeltaZ)) continue;

                Vector3 delta = obstacle.WorldPose.Pose.Position + offset.Position - Origin.WorldPose.Pose.Position;
                float maxDistance = minSurfaceDistance + LaneTracker.Length + obstacle.Length + ObstacleDetectMargin;
                if (maxDistance * maxDistance < delta.LengthSquared()) continue;

                Vector3 localFront = Pose.Transform(obstacle.WorldPose.Pose.Position + offset.Position, poseInv);

                Quaternion relativeRotation = obstacle.WorldPose.Pose.Orientation * Quaternion.Inverse(Origin.WorldPose.Pose.Orientation);
                Vector3 localRight = Vector3.Transform(Vector3.UnitX, relativeRotation) * obstacle.Width / 2;
                Vector3 localUp = Vector3.Transform(Vector3.UnitY, relativeRotation) * obstacle.Height;
                Vector3 localBack = Vector3.Transform(Vector3.UnitZ, relativeRotation) * obstacle.Length;

                Vector3 p1 = localFront - localRight;
                Vector3 p2 = localFront + localRight;
                Vector3 p3 = localFront - localBack - localRight;
                Vector3 p4 = localFront - localBack + localRight;
                Span<Vector3> bboxPoints = [
                    p1, p2, p3, p4,
                    p1 + localUp, p2 + localUp, p3 + localUp, p4 + localUp,
                ];
                BoundingBox bbox = BoundingBox.CreateFromPoints(bboxPoints);

                if (bbox.Max.Z < 0) continue;
                if (bbox.Max.Y < 0 || LaneTracker.Height < bbox.Min.Y) continue;

                float surfaceDistance = float.Max(0, bbox.Min.Z);
                float detectWidth = LaneTracker.Width / 2 + surfaceDistance * 0;
                if (-detectWidth <= bbox.Max.X && bbox.Min.X <= detectWidth && surfaceDistance < minSurfaceDistance)
                {
                    minSurfaceDistance = surfaceDistance;

                    nearestObstacleDistance = 0 <= Vector3.Dot(Origin.WorldPose.Pose.Direction, obstacle.WorldPose.Pose.Direction) ? bbox.Max.Z : bbox.Min.Z;
                    nearestObstacle = new ProjectedEntity(
                        LaneTracker.Heading, LaneTracker.S, Origin.WorldPose.Pose.Direction,
                        obstacle, nearestObstacleDistance, bbox.Width, bbox.Height, bbox.Depth);
                }
            }

            if (float.IsNaN(nearestObstacleDistance))
            {
                Target = null;
                IsTargetOncoming = false;
                DistanceToTarget = float.MaxValue;
            }
            else
            {
                Target = nearestObstacle;
                IsTargetOncoming = LaneTracker.Heading != nearestObstacle.Heading;
                DistanceToTarget = minSurfaceDistance;
            }
        }

        public void Draw(in TransformedDrawContext context)
        {
            if (context.Pass != RenderPass.Traffic) throw new InvalidOperationException();

            DebugVisual.Target = Target;
            DebugVisual.IsTargetOncoming = IsTargetOncoming;
            DebugVisual.Draw(context);
        }


        private readonly struct ProjectedEntity : ITrafficEntity
        {
            private readonly ITrafficEntity Source;

            public readonly WorldPose WorldPose => Source.WorldPose;
            public readonly Vector3 Velocity => Source.Velocity;

            public readonly float Width { get; }
            public readonly float Height { get; }
            public readonly float Length { get; }

            public readonly bool IsEnabled => true;
            public readonly ILanePath? Path => null;
            public readonly EntityDirection Heading { get; }
            public readonly float S { get; }
            public readonly float SVelocity { get; }

            public event Action<ChunkOffset>? Moved
            {
                add => throw new NotSupportedException();
                remove => throw new NotSupportedException();
            }

            public ProjectedEntity(
                EntityDirection originHeading, float originS, Vector3 originDirection,
                ITrafficEntity source, float offset, float width, float height, float length)
            {
                Source = source;

                Width = width;
                Height = height;
                Length = length;

                float dotHeading = Vector3.Dot(originDirection, source.WorldPose.Pose.Direction);
                Heading = 0 <= dotHeading ? originHeading
                    : originHeading == EntityDirection.Forward ? EntityDirection.Backward
                    : EntityDirection.Forward;
                S = originS + (int)originHeading * offset;
                SVelocity = (int)originHeading * Vector3.Dot(originDirection, source.Velocity);
            }

            public readonly bool Spawn(ILanePath path, EntityDirection heading, float s)
            {
                throw new NotSupportedException();
            }
        }
    }
}
