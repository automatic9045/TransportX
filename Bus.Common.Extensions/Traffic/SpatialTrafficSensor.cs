using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;
using Vortice.Mathematics;

using Bus.Common.Rendering;
using Bus.Common.Scenery;
using Bus.Common.Scenery.Networks;
using Bus.Common.Traffic;

using Bus.Common.Extensions.Rendering;

namespace Bus.Common.Extensions.Traffic
{
    public class SpatialTrafficSensor : ITrafficSensor
    {
        private const float ObstacleDetectMargin = 20;


        private readonly ILaneTracker LaneTracker;
        private readonly ILocatable Location;
        private readonly Func<ITrafficParticipant, bool> ObstacleSkipCondition;

        private DynamicLineMesh? DebugMesh = null;

        public float MaxDistance { get; set; } = float.MaxValue;

        public ITrafficParticipant? Target { get; private set; } = null;
        public bool IsTargetOncoming { get; private set; } = false;
        public float DistanceToTarget { get; private set; } = 0;

        public IDebugModel? DebugModel { get; private set; } = null;

        public string? DebugName
        {
            get => field;
            set => DebugModel?.DebugName = field = value;
        }

        public SpatialTrafficSensor(ILaneTracker laneTracker, ILocatable location, Func<ITrafficParticipant, bool> obstacleSkipCondition)
        {
            LaneTracker = laneTracker;
            Location = location;
            ObstacleSkipCondition = obstacleSkipCondition;
        }

        public void Dispose()
        {
            DebugModel?.Dispose();
        }

        public void Tick(IEnumerable<LanePathView> plannedRoute, IEnumerable<ITrafficParticipant> obstacles, TimeSpan elapsed)
        {
            if (!LaneTracker.IsEnabled || LaneTracker.Path is null) throw new InvalidOperationException();

            float minSurfaceDistance = MaxDistance;

            Pose poseInv = Pose.Inverse(Location.Pose);
            ProjectedParticipant nearestObstacle = default;
            float nearestObstacleDistance = float.NaN;
            foreach (ITrafficParticipant obstacle in obstacles)
            {
                if (!obstacle.IsEnabled) continue;
                if (ObstacleSkipCondition(obstacle)) continue;

                PlateOffset offset = Location.GetPlateOffset(obstacle);
                if (1 < int.Abs(offset.DeltaX) || 1 < int.Abs(offset.DeltaZ)) continue;

                Vector3 delta = obstacle.Pose.Position + offset.Position - Location.Pose.Position;
                float maxDistance = minSurfaceDistance + LaneTracker.Length + obstacle.Length + ObstacleDetectMargin;
                if (maxDistance * maxDistance < delta.LengthSquared()) continue;

                Vector3 localFront = Pose.Transform(obstacle.Pose.Position + offset.Position, poseInv);

                Quaternion relativeRotation = obstacle.Pose.Orientation * Quaternion.Inverse(Location.Pose.Orientation);
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

                    float dotDirection = Vector3.Dot(Location.Pose.Direction, obstacle.Pose.Direction);
                    nearestObstacleDistance = 0 <= dotDirection ? bbox.Max.Z : bbox.Min.Z;

                    float physicalLength = bbox.Max.Z - bbox.Min.Z;
                    nearestObstacle = new ProjectedParticipant(
                        LaneTracker.Heading, LaneTracker.S, Location.Pose.Direction, obstacle, nearestObstacleDistance, bbox.Max.X * 2, physicalLength);
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

        public void CreateDebugModel(ID3D11Device device)
        {
            DebugMesh = new(device);
            DebugModel = new WireframeDebugModel([DebugMesh]);
            DebugName = DebugName;
        }

        public void DrawDebug(LocatedDrawContext context)
        {
            if (DebugMesh is null || DebugModel is null) throw new InvalidOperationException();
            if (Target is null) return;

            VertexConstantBuffer vertexBuffer = new()
            {
                World = Matrix4x4.Transpose((Location.Pose * context.PlateOffset.Pose).ToMatrix4x4()),
                View = Matrix4x4.Transpose(context.View),
                Projection = Matrix4x4.Transpose(context.Projection),
                Light = context.Light.AsVector4(),
            };
            context.DeviceContext.UpdateSubresource(vertexBuffer, context.VertexConstantBuffer);

            float lengthShift = IsTargetOncoming ? 0 : Target.Length;
            Vector3 worldDelta = Target.Pose.Position - Target.Pose.Direction * lengthShift + Location.GetPlateOffset(Target).Position - Location.Pose.Position;
            Vector3 localDelta = Vector3.Transform(worldDelta, Quaternion.Inverse(Location.Pose.Orientation));

            DebugMesh.SetVector(context.DeviceContext, localDelta);
            DebugModel.Draw(new(context.DeviceContext, context.VertexConstantBuffer, context.PixelConstantBuffer));
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
