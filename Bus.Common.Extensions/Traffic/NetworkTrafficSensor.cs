using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

using Bus.Common.Rendering;
using Bus.Common.Scenery;
using Bus.Common.Scenery.Networks;
using Bus.Common.Traffic;

using Bus.Common.Extensions.Rendering;

namespace Bus.Common.Extensions.Traffic
{
    public class NetworkTrafficSensor : ITrafficSensor
    {
        private readonly ILaneTracker LaneTracker;
        private readonly ILocatable Location;

        private DynamicLineMesh? DebugMesh = null;

        public ITrafficParticipant? Target { get; private set; } = null;
        public bool IsTargetOncoming { get; private set; } = false;
        public float DistanceToTarget { get; private set; } = 0;

        public IDebugModel? DebugModel { get; private set; } = null;

        public string? DebugName
        {
            get => field;
            set => DebugModel?.DebugName = field = value;
        }

        public NetworkTrafficSensor(ILaneTracker laneTracker, ILocatable location)
        {
            LaneTracker = laneTracker;
            Location = location;
        }

        public void Dispose()
        {
            DebugModel?.Dispose();
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
    }
}
