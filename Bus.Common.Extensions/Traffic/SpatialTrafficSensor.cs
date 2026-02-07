using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;

using Bus.Common.Scenery;
using Bus.Common.Scenery.Networks;
using Bus.Common.Traffic;

namespace Bus.Common.Extensions.Traffic
{
    public class SpatialTrafficSensor : ITrafficSensor
    {
        private const float ObstacleDetectMargin = 20;


        private readonly ILaneTracker LaneTracker;
        private readonly IPoseSolver PoseSolver;
        private readonly Func<ITrafficParticipant, bool> ObstacleSkipCondition;

        public float MaxDistance { get; set; } = float.MaxValue;

        public ITrafficParticipant? Target { get; private set; } = null;
        public bool IsTargetOncoming { get; private set; } = false;
        public float DistanceToTarget { get; private set; } = 0;

        public SpatialTrafficSensor(ILaneTracker laneTracker, IPoseSolver poseSolver, Func<ITrafficParticipant, bool> obstacleSkipCondition)
        {
            LaneTracker = laneTracker;
            PoseSolver = poseSolver;
            ObstacleSkipCondition = obstacleSkipCondition;
        }

        public void Tick(IEnumerable<LanePathView> plannedRoute, IEnumerable<ITrafficParticipant> obstacles, TimeSpan elapsed)
        {
            if (!LaneTracker.IsEnabled || LaneTracker.Path is null) throw new InvalidOperationException();

            float minSurfaceDistance = MaxDistance;

            Pose poseInv = Pose.Inverse(PoseSolver.Pose);
            ProjectedParticipant nearestObstacle = default;
            float nearestObstacleDistance = float.NaN;
            foreach (ITrafficParticipant obstacle in obstacles)
            {
                if (!obstacle.IsEnabled) continue;
                if (ObstacleSkipCondition(obstacle)) continue;

                PlateOffset offset = PoseSolver.GetPlateOffset(obstacle);
                if (1 < int.Abs(offset.DeltaX) || 1 < int.Abs(offset.DeltaZ)) continue;

                Vector3 delta = obstacle.Pose.Position + offset.Position - PoseSolver.Pose.Position;
                float maxDistance = minSurfaceDistance + LaneTracker.Length + obstacle.Length + ObstacleDetectMargin;
                if (maxDistance * maxDistance < delta.LengthSquared()) continue;

                Vector3 localFront = Pose.Transform(obstacle.Pose.Position + offset.Position, poseInv);

                Quaternion relativeRotation = obstacle.Pose.Orientation * Quaternion.Inverse(PoseSolver.Pose.Orientation);
                Vector3 localRight = Vector3.Transform(Vector3.UnitX, relativeRotation) * obstacle.Width / 2;
                Vector3 localBack = Vector3.Transform(Vector3.UnitZ, relativeRotation) * obstacle.Length;

                Span<Vector3> bboxPoints = [
                     localFront - localRight,
                    localFront + localRight,
                    localFront - localBack - localRight,
                    localFront - localBack + localRight,
                ];
                BoundingBox bbox = BoundingBox.CreateFromPoints(bboxPoints);

                if (bbox.Max.Z < 0) continue;

                float surfaceDistance = float.Max(0, bbox.Min.Z);
                if (-LaneTracker.Width / 2 <= bbox.Max.X && bbox.Min.X <= LaneTracker.Width / 2 && surfaceDistance < minSurfaceDistance)
                {
                    minSurfaceDistance = surfaceDistance;

                    float dotDirection = Vector3.Dot(PoseSolver.Pose.Direction, obstacle.Pose.Direction);
                    nearestObstacleDistance = 0 <= dotDirection ? bbox.Max.Z : bbox.Min.Z;

                    float physicalLength = bbox.Max.Z - bbox.Min.Z;
                    nearestObstacle = new ProjectedParticipant(
                        LaneTracker.Heading, LaneTracker.S, PoseSolver.Pose.Direction, obstacle, nearestObstacleDistance, bbox.Max.X * 2, physicalLength);
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
                DistanceToTarget = nearestObstacleDistance;
            }
        }


        private readonly struct ProjectedParticipant : ITrafficParticipant
        {
            private readonly ITrafficParticipant Source;

            public readonly int PlateX => Source.PlateX;
            public readonly int PlateZ => Source.PlateZ;
            public readonly Pose Pose => Source.Pose;
            public readonly Vector3 Velocity => Source.Velocity;

            public readonly float Width { get; }
            public readonly float Length { get; }

            public readonly bool IsEnabled => true;
            public readonly ILanePath? Path => null;
            public readonly ParticipantDirection Heading { get; }
            public readonly float S { get; }
            public readonly float SVelocity { get; }

            public event EventHandler? Moved
            {
                add => throw new NotSupportedException();
                remove => throw new NotSupportedException();
            }

            public ProjectedParticipant(
                ParticipantDirection originHeading, float originS, Vector3 originDirection, ITrafficParticipant source, float offset, float width, float length)
            {
                Source = source;

                Width = width;
                Length = length;

                float dotHeading = Vector3.Dot(originDirection, source.Pose.Direction);
                Heading = 0 <= dotHeading ? originHeading
                    : originHeading == ParticipantDirection.Forward ? ParticipantDirection.Backward
                    : ParticipantDirection.Forward;
                S = originS + (int)originHeading * offset;
                SVelocity = (int)originHeading * Vector3.Dot(originDirection, source.Velocity);
            }

            public readonly bool Spawn(ILanePath path, ParticipantDirection heading, float s)
            {
                throw new NotSupportedException();
            }
        }
    }
}
