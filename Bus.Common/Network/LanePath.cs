using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.Mathematics;

using Bus.Common.Rendering;
using Bus.Common.Traffic;

namespace Bus.Common.Network
{
    public abstract class LanePath : ILanePath
    {
        protected const float SweepBack = 0.5f;


        protected readonly LaneWidth FromWidth;

        protected Material? DebugSpineMaterial = null;
        protected Material? DebugWingMaterial = null;
        protected LanePathDebugModel? DebugModel = null;

        public NetworkElement Owner => From.Port.Owner;
        public LaneTrafficGroup AllowedTraffic => From.Definition.AllowedTraffic;
        public FlowDirections Directions => From.Definition.Directions.GetOpposition();

        public LanePin From { get; }
        public LanePin To { get; }

        private readonly List<ITrafficParticipant> ParticipantsKey = [];
        public IReadOnlyList<ITrafficParticipant> Participants => ParticipantsKey;

        public abstract float Length { get; }

        public Vector4 DebugColor { get; set; } = Vector4.One;
        public string? DebugName
        {
            get => field;
            set
            {
                field = value;
                DebugModel?.DebugName = value is null ? null : $"{value}_LanePath";
            }
        } = null;

        protected LanePath(LanePin from, LanePin to)
        {
            // TODO: Kind、Directionsの検証
            if (from.Port.Owner != to.Port.Owner) throw new ArgumentException($"同一 {nameof(NetworkElement)} 内のピンを指定する必要があります。", nameof(to));

            From = from;
            To = to;
            FromWidth = LaneWidth.Opposition(From.Definition.Width);
        }

        public virtual void Dispose()
        {
            DebugModel?.Dispose();
        }

        public abstract Pose GetLocalPose(float at);

        public Pose GetPose(float at)
        {
            return GetLocalPose(at) * Owner.Pose;
        }

        public abstract LaneWidth GetWidth(float at);

        public virtual void Enter(ITrafficParticipant participant)
        {
            ParticipantsKey.Add(participant);
        }

        public virtual void Exit(ITrafficParticipant participant)
        {
            ParticipantsKey.Remove(participant);
        }

        public void Draw(LocatedDrawContext context)
        {
            if (context.Pass != RenderPass.Network) throw new InvalidOperationException();

            if (DebugModel is null)
            {
                int stepCount = int.Max(1, (int)float.Ceiling(Length));

                List<Vertex> spineVertices = [];
                List<int> spineIndices = [];

                List<Vertex> wingVertices = [];
                List<int> wingIndices = [];

                int? prevSpineIndex = null;

                for (int i = 0; i <= stepCount; i++)
                {
                    float s = (i == stepCount) ? Length : (float)i;

                    Pose pose = GetLocalPose(s);

                    int currentSpineIndex = spineVertices.Count;
                    spineVertices.Add(new Vertex(pose.Position, Vector4.One));

                    if (prevSpineIndex.HasValue)
                    {
                        spineIndices.Add(prevSpineIndex.Value);
                        spineIndices.Add(currentSpineIndex);
                    }
                    prevSpineIndex = currentSpineIndex;

                    if (SweepBack <= s)
                    {
                        float sWing = s - SweepBack;
                        Pose wingPose = GetLocalPose(sWing);
                        LaneWidth wingWidth = GetWidth(sWing);

                        Vector3 wingRight = Pose.TransformNormal(Vector3.UnitX, wingPose);

                        int leftIndex = wingVertices.Count;
                        wingVertices.Add(new Vertex(wingPose.Position - wingRight * wingWidth.Left, Vector4.One));

                        int rightIndex = wingVertices.Count;
                        wingVertices.Add(new Vertex(wingPose.Position + wingRight * wingWidth.Right, Vector4.One));

                        int tipIndex = wingVertices.Count;
                        wingVertices.Add(new Vertex(pose.Position, Vector4.One));

                        wingIndices.Add(leftIndex);
                        wingIndices.Add(tipIndex);

                        wingIndices.Add(rightIndex);
                        wingIndices.Add(tipIndex);
                    }
                }

                DebugSpineMaterial = new Material(default, []);
                DebugWingMaterial = new Material(default, []);

                Mesh spineMesh = Mesh.Create(context.DeviceContext.Device, spineVertices.ToArray(), spineIndices.ToArray(), DebugSpineMaterial, PrimitiveTopology.LineList);
                Mesh wingMesh = Mesh.Create(context.DeviceContext.Device, wingVertices.ToArray(), wingIndices.ToArray(), DebugWingMaterial, PrimitiveTopology.LineList);

                DebugModel = new LanePathDebugModel(spineMesh, wingMesh);
            }

            VertexConstantBuffer vertexBuffer = new()
            {
                World = Matrix4x4.Transpose((Owner.Pose * context.PlateOffset.Pose).ToMatrix4x4()),
                View = Matrix4x4.Transpose(context.View),
                Projection = Matrix4x4.Transpose(context.Projection),
                Light = context.Light.AsVector4(),
            };
            context.DeviceContext.UpdateSubresource(vertexBuffer, context.VertexConstantBuffer);

            DebugSpineMaterial!.BaseColor = DebugColor;
            DebugWingMaterial!.BaseColor = new Vector4(DebugColor.AsVector3(), DebugColor.W * 0.3f);
            DebugModel.Draw(new(context.DeviceContext, context.VertexConstantBuffer, context.PixelConstantBuffer));
        }
    }
}
