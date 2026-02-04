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

namespace Bus.Common.Scenery.Networks
{
    public abstract class LanePath : ILanePath
    {
        protected const float SweepBack = 0.5f;


        protected readonly LaneWidth FromWidth;

        protected readonly Material DebugSpineMaterial = new(Vector4.One, []);
        protected readonly Material DebugWingMaterial = new(new Vector4(1, 1, 1, 0.5f), []);

        protected ID3D11DepthStencilState? NoDepthState = null;
        protected ID3D11RasterizerState? DebugRasterizerState = null;
        protected ID3D11BlendState? AlphaBlendState = null;

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
            get => DebugSpineMaterial.BaseColor;
            set
            {
                DebugSpineMaterial.BaseColor = value;
                DebugWingMaterial.BaseColor = new Vector4(value.AsVector3(), value.W * 0.3f);
            }
        }
        public IModel? DebugModel { get; protected set; } = null;

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
            NoDepthState?.Dispose();
            DebugRasterizerState?.Dispose();
            AlphaBlendState?.Dispose();
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

        public virtual void CreateDebugResources(ID3D11Device device)
        {
            if (DebugModel is not null) throw new InvalidOperationException("モデルは既に作成されています。");

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

            Mesh spineMesh = Mesh.Create(device, spineVertices.ToArray(), spineIndices.ToArray(), DebugSpineMaterial, PrimitiveTopology.LineList);
            Mesh wingMesh = Mesh.Create(device, wingVertices.ToArray(), wingIndices.ToArray(), DebugWingMaterial, PrimitiveTopology.LineList);

            DebugModel = new Model([spineMesh, wingMesh], []);
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

            if (AlphaBlendState is null)
            {
                BlendDescription desc = new BlendDescription();
                desc.RenderTarget[0] = new RenderTargetBlendDescription()
                {
                    BlendEnable = true,
                    SourceBlend = Blend.SourceAlpha,
                    DestinationBlend = Blend.InverseSourceAlpha,
                    BlendOperation = BlendOperation.Add,
                    SourceBlendAlpha = Blend.One,
                    DestinationBlendAlpha = Blend.Zero,
                    BlendOperationAlpha = BlendOperation.Add,
                    RenderTargetWriteMask = ColorWriteEnable.All,
                };
                AlphaBlendState = context.DeviceContext.Device.CreateBlendState(desc);
            }

            context.DeviceContext.OMGetDepthStencilState(out ID3D11DepthStencilState? oldDState, out uint oldRef);
            ID3D11BlendState? oldBState = context.DeviceContext.OMGetBlendState(out Color4 oldBFactor, out uint oldBMask);
            ID3D11RasterizerState? oldRSState = context.DeviceContext.RSGetState();

            context.DeviceContext.OMSetDepthStencilState(NoDepthState);
            context.DeviceContext.OMSetBlendState(AlphaBlendState);
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
            context.DeviceContext.OMSetBlendState(oldBState, oldBFactor, oldBMask);
            context.DeviceContext.RSSetState(oldRSState);

            oldDState?.Dispose();
            oldBState?.Dispose();
            oldRSState?.Dispose();
        }
    }
}
