using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Rendering;
using TransportX.Traffic;

using TransportX.Extensions.Rendering;

namespace TransportX.Extensions.Traffic
{
    public class TrafficSensorDebugVisual : IDebugDrawable
    {
        private readonly ILocatable Location;

        private DynamicLineMesh? Mesh = null;
        private WireframeDebugModel? Model = null;

        public ITrafficParticipant? Target { get; set; } = null;
        public bool IsTargetOncoming { get; set; } = false;

        public Vector4 DebugColor { get; set; } = Vector4.One;
        public string? DebugName
        {
            get => field;
            set => Model?.DebugName = field = value;
        }

        public TrafficSensorDebugVisual(ILocatable location)
        {
            Location = location;
        }

        public void Dispose()
        {
            Model?.Dispose();
        }

        public void Draw(in LocatedDrawContext context)
        {
            if (context.Pass != RenderPass.Traffic) throw new InvalidOperationException();
            if (Target is null) return;

            if (Model is null)
            {
                Mesh = new DynamicLineMesh(context.DeviceContext.Device, Material.Default());
                Model = new WireframeDebugModel([Mesh]);
                DebugName = DebugName;
            }

            InstanceData instanceData = new()
            {
                World = Matrix4x4.Transpose((Location.Pose * context.PlateOffset.Pose).ToMatrix4x4()),
            };

            float lengthShift = IsTargetOncoming ? 0 : Target.Length;
            Vector3 worldDelta = Target.Pose.Position - Target.Pose.Direction * lengthShift + Location.GetPlateOffset(Target).Position - Location.Pose.Position;
            Vector3 localDelta = Vector3.Transform(worldDelta, Quaternion.Inverse(Location.Pose.Orientation));

            Mesh!.Material.BaseColor = DebugColor.ToLinear();
            Mesh.SetVector(context.DeviceContext, localDelta);

            context.RenderQueue.Submit(context.Pass, Model, instanceData);
        }
    }
}
