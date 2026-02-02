using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D;
using Vortice.Direct3D11;

using Bus.Common.Rendering;
using Bus.Common.Traffic;

namespace Bus.Common.Scenery.Networks
{
    public abstract class LanePath : IDebugVisualizable
    {
        private const float DebugModelWidth = 0.25f;


        protected readonly Material DebugMaterial = new(Vector4.One, []);

        protected ID3D11DepthStencilState? NoDepthState = null;
        protected ID3D11RasterizerState? DebugRasterizerState = null;

        public NetworkElement Owner => From.Port.Owner;
        public LaneTrafficGroup AllowedTraffic => From.Definition.AllowedTraffic;
        public FlowDirections Directions => From.Definition.Directions.GetOpposition();

        public LanePin From { get; }
        public LanePin To { get; }

        private readonly List<ITrafficParticipant> ParticipantsKey = [];
        public IReadOnlyList<ITrafficParticipant> Participants => ParticipantsKey;

        public abstract float Length { get; }

        public string? DebugName
        {
            get => field;
            set
            {
                field = value;
                DebugModel?.DebugName = value is null ? null : $"{value}_LanePath";
            }
        } = null;

        public bool CanDrawDebug => DebugModel is not null;
        public virtual Vector4 DebugColor
        {
            get => DebugMaterial.BaseColor;
            set => DebugMaterial.BaseColor = value;
        }
        public IModel? DebugModel { get; protected set; } = null;

        protected LanePath(LanePin from, LanePin to)
        {
            // TODO: Kind、Directionsの検証
            if (from.Port.Owner != to.Port.Owner) throw new ArgumentException($"同一 {nameof(NetworkElement)} 内のピンを指定する必要があります。", nameof(to));

            From = from;
            To = to;
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

        public virtual void Enter(ITrafficParticipant participant)
        {
            ParticipantsKey.Add(participant);
        }

        public virtual void Exit(ITrafficParticipant participant)
        {
            ParticipantsKey.Remove(participant);
        }

        public virtual void CreateDebugResources(ID3D11Device device)
        {
            if (DebugModel is not null) throw new InvalidOperationException("モデルは既に作成されています。");

            int stepCount = int.Max(1, (int)float.Ceiling(Length));

            List<Vertex> vertices = [];
            List<int> indices = [];
            for (int i = 0; i <= stepCount; i++)
            {
                Pose pose = GetLocalPose(i == stepCount ? Length : i);

                Vector3 right = Pose.TransformNormal(Vector3.UnitX * DebugModelWidth, pose);

                vertices.Add(new Vertex(pose.Position, Vector4.One));
                vertices.Add(new Vertex(pose.Position + right, Vector4.One));

                indices.Add(i * 2);
                indices.Add(i * 2 + 1);

                if (0 < i)
                {
                    indices.Add((i - 1) * 2);
                    indices.Add(i * 2);
                }
            }

            Mesh visualMesh = Mesh.Create(device, vertices.ToArray(), indices.ToArray(), DebugMaterial, PrimitiveTopology.LineList);
            DebugModel = new Model([visualMesh], []);
        }

        public void DrawDebug(LocatedDrawContext context)
        {
            if (DebugModel is null) throw new InvalidOperationException("デバッグモデルが作成されていません。");

            if (NoDepthState is null)
            {
                DepthStencilDescription desc = new()
                {
                    DepthEnable = false,
                    DepthWriteMask = DepthWriteMask.All,
                    DepthFunc = ComparisonFunction.Always,
                    StencilEnable = false,
                };
                NoDepthState = context.DeviceContext.Device.CreateDepthStencilState(desc);
            }

            if (DebugRasterizerState is null)
            {
                RasterizerDescription desc = new RasterizerDescription()
                {
                    CullMode = CullMode.None,
                    FillMode = FillMode.Wireframe,
                    DepthClipEnable = true,
                };
                DebugRasterizerState = context.DeviceContext.Device.CreateRasterizerState(desc);
            }

            context.DeviceContext.OMGetDepthStencilState(out ID3D11DepthStencilState? oldDState, out uint oldRef);
            context.DeviceContext.OMSetDepthStencilState(NoDepthState, 0);

            ID3D11RasterizerState? oldRSState = context.DeviceContext.RSGetState();
            context.DeviceContext.RSSetState(DebugRasterizerState);

            VertexConstantBuffer vertexBuffer = new()
            {
                World = Matrix4x4.Transpose((Owner.Pose * context.PlateOffset.Pose).ToMatrix4x4()),
                View = Matrix4x4.Transpose(context.View),
                Projection = Matrix4x4.Transpose(context.Projection),
                Light = context.Light.AsVector4(),
            };
            context.DeviceContext.UpdateSubresource(vertexBuffer, context.VertexConstantBuffer);

            DebugModel.Draw(new(context.DeviceContext, context.VertexConstantBuffer, context.PixelConstantBuffer));

            context.DeviceContext.OMSetDepthStencilState(oldDState, oldRef);
            oldDState?.Dispose();

            context.DeviceContext.RSSetState(oldRSState);
            oldRSState?.Dispose();
        }
    }
}
