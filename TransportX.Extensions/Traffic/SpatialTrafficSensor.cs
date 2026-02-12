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

using TransportX.Extensions.Rendering;

namespace TransportX.Extensions.Traffic
{
    public class SpatialTrafficSensor : ITrafficSensor
    {
        private const float ObstacleDetectMargin = 20;


        private readonly ILaneTracker LaneTracker;
        private readonly ILocatable Location;
        private readonly Func<ITrafficParticipant, bool> ObstacleSkipCondition;

        private DynamicLineMesh? DebugMesh = null;
        private WireframeDebugModel? DebugModel = null;

        public float MaxDistance { get; set; } = float.MaxValue;

        public ITrafficParticipant? Target { get; private set; } = null;
        public bool IsTargetOncoming { get; private set; } = false;
        public float DistanceToTarget { get; private set; } = 0;

        public Vector4 DebugColor { get; set; } = Vector4.One;
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
                if (-LaneTracker.Width / 2 <= bbox.Max.X && bbox.Min.X <= LaneTracker.Width / 2 && surfaceDistance < minSurfaceDistance)
                {
                    minSurfaceDistance = surfaceDistance;

                    nearestObstacleDistance = 0 <= Vector3.Dot(Location.Pose.Direction, obstacle.Pose.Direction) ? bbox.Max.Z : bbox.Min.Z;
                    nearestObstacle = new ProjectedParticipant(
                        LaneTracker.Heading, LaneTracker.S, Location.Pose.Direction,
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
                DistanceToTarget = nearestObstacleDistance;
            }
        }

        public void Draw(LocatedDrawContext context)
        {
            if (context.Pass != RenderPass.Traffic) throw new InvalidOperationException();
            if (Target is null) return;

            if (DebugModel is null)
            {
                DebugMesh = new DynamicLineMesh(context.DeviceContext.Device, new Material(default, []));
                DebugModel = new WireframeDebugModel([DebugMesh]);
                DebugName = DebugName;
            }

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

            DebugMesh!.Material.BaseColor = DebugColor.ToLinear();
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
            public readonly float Height { get; }
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
                ParticipantDirection originHeading, float originS, Vector3 originDirection,
                ITrafficParticipant source, float offset, float width, float height, float length)
            {
                Source = source;

                Width = width;
                Height = height;
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
