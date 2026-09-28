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
        private readonly ITrafficRegistry Registry;
        private readonly Func<ITrafficEntity, bool> ObstacleSkipCondition;
        private readonly TrafficSensorDebugVisual DebugVisual;

        private readonly ProjectedEntity EntityCache = new();

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

        public SpatialTrafficSensor(ILaneTracker laneTracker, IWorldObject origin, ITrafficRegistry registry, Func<ITrafficEntity, bool> obstacleSkipCondition)
        {
            LaneTracker = laneTracker;
            Origin = origin;
            Registry = registry;
            ObstacleSkipCondition = obstacleSkipCondition;

            DebugVisual = new TrafficSensorDebugVisual(Origin);
        }

        public void Dispose()
        {
            DebugVisual.Dispose();
        }

        public void Tick(IReadOnlyList<LanePathView> plannedRoute, TimeSpan elapsed)
        {
            if (!LaneTracker.IsEnabled || LaneTracker.Path is null) throw new InvalidOperationException();

            float minSurfaceDistance = MaxDistance;

            Pose poseInv = Pose.Inverse(Origin.WorldPose.Pose);
            float nearestObstacleDistance = float.NaN;
            for (int dx = -1; dx < 2; dx++)
            {
                for (int dz = -1; dz < 2; dz++)
                {
                    ChunkIndex chunkOffset = new(dx, dz);
                    ChunkIndex targetChunk = Origin.WorldPose.Chunk + chunkOffset;
                    IReadOnlyList<ITrafficEntity> obstacles = Registry.GetEntitiesInChunk(targetChunk);

                    for (int i = 0; i < obstacles.Count; i++)
                    {
                        ITrafficEntity obstacle = obstacles[i];
                        if (!obstacle.IsEnabled) continue;
                        if (ObstacleSkipCondition(obstacle)) continue;

                        Vector3 delta = obstacle.WorldPose.Pose.Position + chunkOffset.Position - Origin.WorldPose.Pose.Position;
                        float maxDistance = minSurfaceDistance + LaneTracker.Length + obstacle.Length + ObstacleDetectMargin;
                        if (maxDistance * maxDistance < delta.LengthSquared()) continue;

                        Vector3 obstacleFrontPosition = obstacle.WorldPose.Pose.Position + chunkOffset.Position;
                        Vector3 obstacleRearPosition = obstacleFrontPosition - obstacle.WorldPose.Pose.Direction * obstacle.Length;

                        Vector3 localFront = Pose.Transform(obstacleFrontPosition, poseInv);
                        Vector3 localRear = Pose.Transform(obstacleRearPosition, poseInv);

                        if (float.Max(localFront.Z, localRear.Z) < 0) continue;

                        float obstacleMinZ = float.Min(localFront.Z, localRear.Z);
                        float advanceDistance = float.Max(0, obstacleMinZ);

                        if (!GetFutureLocalPose(plannedRoute, LaneTracker.S, advanceDistance, Origin.WorldPose.Chunk, out Pose futurePose)) continue;
                        Pose futurePoseInv = Pose.Inverse(futurePose);

                        Vector3 futureLocalFront = Pose.Transform(obstacle.WorldPose.Pose.Position + chunkOffset.Position, futurePoseInv);
                        if (futureLocalFront.Z < -obstacle.Length) continue;

                        Quaternion relativeRotation = obstacle.WorldPose.Pose.Orientation * Quaternion.Inverse(futurePose.Orientation);

                        Vector3 localRight = Vector3.Transform(Vector3.UnitX, relativeRotation) * obstacle.Width / 2;
                        Vector3 localUp = Vector3.Transform(Vector3.UnitY, relativeRotation) * obstacle.Height;
                        Vector3 localBack = Vector3.Transform(Vector3.UnitZ, relativeRotation) * obstacle.Length;

                        Vector3 p1 = futureLocalFront - localRight;
                        Vector3 p2 = futureLocalFront + localRight;
                        Vector3 p3 = futureLocalFront - localBack - localRight;
                        Vector3 p4 = futureLocalFront - localBack + localRight;
                        Span<Vector3> bboxPoints = [
                            p1, p2, p3, p4,
                            p1 + localUp, p2 + localUp, p3 + localUp, p4 + localUp,
                        ];
                        BoundingBox bbox = BoundingBox.CreateFromPoints(bboxPoints);

                        if (advanceDistance + bbox.Max.Z < 0) continue;
                        if (bbox.Max.Y < 0 || LaneTracker.Height < bbox.Min.Y) continue;

                        float surfaceDistance = float.Max(0, advanceDistance + bbox.Min.Z);
                        float detectWidth = LaneTracker.Width / 2 + 0.2f;
                        if (-detectWidth <= bbox.Max.X && bbox.Min.X <= detectWidth && surfaceDistance < minSurfaceDistance)
                        {
                            minSurfaceDistance = surfaceDistance;

                            nearestObstacleDistance = advanceDistance + futureLocalFront.Z;
                            EntityCache.Update(LaneTracker.Heading, LaneTracker.S, futurePose.Direction, obstacle, nearestObstacleDistance);
                        }
                    }
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
                Target = EntityCache;
                IsTargetOncoming = LaneTracker.Heading != EntityCache.Heading;
                DistanceToTarget = minSurfaceDistance;
            }
        }

        private bool GetFutureLocalPose(IReadOnlyList<LanePathView> plannedRoute, float startS, float advanceDistance, ChunkIndex originChunk, out Pose localPose)
        {
            float remainingDistance = advanceDistance;
            LanePathView currentView = new(LaneTracker.Path!, LaneTracker.Heading);
            float currentViewS = currentView.ToViewS(startS);
            float currentAvailable = currentView.Source.Length - currentViewS;

            if (remainingDistance <= currentAvailable)
            {
                WorldPose wp = currentView.GetWorldPose(currentViewS + remainingDistance);
                ChunkIndex offset = wp.Chunk - originChunk;

                localPose = new Pose(wp.Pose.Position + offset.Position, wp.Pose.Orientation);
                return true;
            }

            remainingDistance -= currentAvailable;

            for (int i = 0; i < plannedRoute.Count; i++)
            {
                LanePathView view = plannedRoute[i];

                float availableDistance = view.Source.Length;
                if (remainingDistance <= availableDistance)
                {
                    WorldPose wp = view.GetWorldPose(remainingDistance);
                    ChunkIndex offset = wp.Chunk - originChunk;

                    localPose = new Pose(wp.Pose.Position + offset.Position, wp.Pose.Orientation);
                    return true;
                }
                remainingDistance -= availableDistance;
            }

            localPose = Pose.Identity;
            return false;
        }

        public void Draw(in TransformedDrawContext context)
        {
            if (context.Layer != RenderLayer.Traffic) throw new InvalidOperationException();

            DebugVisual.Target = Target;
            DebugVisual.IsTargetOncoming = IsTargetOncoming;
            DebugVisual.Draw(context);
        }


        private sealed class ProjectedEntity : ITrafficEntity
        {
            private ITrafficEntity? Source;

            public WorldPose WorldPose => Source is null ? WorldPose.Zero : Source.WorldPose;
            public Vector3 Velocity => Source is null ? Vector3.Zero : Source.Velocity;
            public Vector3 AngularVelocity => Source is null ? Vector3.Zero : Source.AngularVelocity;

            public float Width => Source is null ? 0 : Source.Width;
            public float Height => Source is null ? 0 : Source.Height;
            public float Length => Source is null ? 0 : Source.Length;

            public bool IsEnabled => true;
            public ILanePath? Path => null;
            public EntityDirection Heading { get; private set; }
            public float S { get; private set; }
            public float SVelocity { get; private set; }

            public event MovedEventHandler? Moved
            {
                add => throw new NotSupportedException();
                remove => throw new NotSupportedException();
            }

            public ProjectedEntity()
            {
            }

            public void Update(EntityDirection originHeading, float originS, Vector3 originDirection, ITrafficEntity source, float offset)
            {
                Source = source;

                float dotHeading = Vector3.Dot(originDirection, source.WorldPose.Pose.Direction);
                Heading = 0 <= dotHeading ? originHeading
                    : originHeading == EntityDirection.Forward ? EntityDirection.Backward
                    : EntityDirection.Forward;
                S = originS + (int)originHeading * offset;
                SVelocity = (int)originHeading * Vector3.Dot(originDirection, source.Velocity);
            }

            public bool Spawn(ILanePath path, EntityDirection heading, float s)
            {
                throw new NotSupportedException();
            }
        }
    }
}
