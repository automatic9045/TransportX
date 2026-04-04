using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;
using TransportX.Diagnostics;
using TransportX.Network;
using TransportX.Rendering;
using TransportX.Spatial;

using TransportX.Extensions.Mathematics;
using TransportX.Extensions.Network.Elements;
using TransportX.Extensions.Network.Paths;

namespace TransportX.Scripting.Commands
{
    public class JunctionPathTemplate
    {
        private static PortDefinition EmptyPort() => new PortDefinition(string.Empty, new LaneLayout(), Pose.Identity);
        internal static JunctionPathTemplate Empty(ScriptWorld world, JunctionTemplate parent) => new(world, parent, string.Empty, EmptyPort(), 0, EmptyPort(), 0);


        private readonly PortDefinition FromPort;
        private readonly int FromPinIndex;
        private readonly PortDefinition ToPort;
        private readonly int ToPinIndex;

        private readonly List<PoseCurveBase> Curves = [];

        private float Length = 0;
        private bool IsFinalized = false;

        public string Key { get; }

        private Pose LastCurvePoint => Curves.Count == 0 ? Pose.CreateRotationY(float.Pi) * FromPort.GetPinLocalPose(FromPinIndex) : Curves[^1].To;
        public WidthPointList Width { get; }

        private readonly List<SplineStructure> StructuresKey = [];
        public IReadOnlyList<SplineStructure> Structures => StructuresKey;

        public ScriptWorld World { get; }
        public JunctionTemplate Parent { get; }
        public IComponentCollection<ITemplateComponent<ILanePath>> Components { get; } = new ComponentCollection<ITemplateComponent<ILanePath>>();

        public JunctionPathTemplate(ScriptWorld world, JunctionTemplate parent, string key, PortDefinition fromPort, int fromPinIndex, PortDefinition toPort, int toPinIndex)
        {
            World = world;
            Parent = parent;

            Key = key;
            FromPort = fromPort;
            FromPinIndex = fromPinIndex;
            ToPort = toPort;
            ToPinIndex = toPinIndex;

            Width = new(LaneWidth.Opposition(FromPort.Layout.Lanes[FromPinIndex].Width));
        }

        private bool CheckNotFinalized()
        {
            if (IsFinalized)
            {
                ScriptError error = new(ErrorLevel.Error, "これ以上セグメントを追加することはできません。");
                World.ErrorCollector.Report(error);
                return false;
            }

            return true;
        }

        private Pose CreateCurvePoint(double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return position.ToPose();
        }

        public JunctionPathTemplate StraightTo(double x, double y, double z, double rotationX, double rotationY, double rotationZ, out double s)
        {
            if (!CheckNotFinalized())
            {
                s = Length;
                return this;
            }

            Pose to = CreateCurvePoint(x, y, z, rotationX, rotationY, rotationZ);
            LinearPoseCurve curve = new(LastCurvePoint, to);
            Curves.Add(curve);

            Length += curve.Length;
            s = Length;
            return this;
        }

        public JunctionPathTemplate BezierTo(double x, double y, double z, double rotationX, double rotationY, double rotationZ, out double s, double? controlScale = null)
        {
            if (!CheckNotFinalized())
            {
                s = Length;
                return this;
            }

            Pose to = CreateCurvePoint(x, y, z, rotationX, rotationY, rotationZ);
            BezierPoseCurve curve = new(LastCurvePoint, to, (float?)controlScale);
            Curves.Add(curve);

            Length += curve.Length;
            s = Length;
            return this;
        }

        public void StraightToEnd(out double s)
        {
            if (!CheckNotFinalized())
            {
                s = Length;
                return;
            }
            IsFinalized = true;

            Pose to = ToPort.GetPinLocalPose(ToPinIndex);
            LinearPoseCurve curve = new(LastCurvePoint, to);
            Curves.Add(curve);

            Length += curve.Length;
            s = Length;
        }

        public void BezierToEnd(out double s, double? controlScale = null)
        {
            if (!CheckNotFinalized())
            {
                s = Length;
                return;
            }
            IsFinalized = true;

            Pose to = ToPort.GetPinLocalPose(ToPinIndex);
            BezierPoseCurve curve = new(LastCurvePoint, to, (float?)controlScale);
            Curves.Add(curve);

            Length += curve.Length;
            s = Length;
        }

        public JunctionPathTemplate StraightTo(double x, double y, double z, double rotationX, double rotationY, double rotationZ)
            => StraightTo(x, y, z, rotationX, rotationY, rotationZ, out _);
        public JunctionPathTemplate BezierTo(double x, double y, double z, double rotationX, double rotationY, double rotationZ, double? controlScale = null)
            => BezierTo(x, y, z, rotationX, rotationY, rotationZ, out _, controlScale);
        public void StraightToEnd() => StraightToEnd(out _);
        public void BezierToEnd(double? controlScale = null) => BezierToEnd(out _, controlScale);

        public SplineStructure PutStructure(IReadOnlyList<string> modelKeys, Pose pose, double from, double span, double interval, int count = int.MaxValue)
        {
            LocatedModelTemplate[] models = modelKeys.Select(key =>
            {
                IModel? model;
                if (key == string.Empty)
                {
                    model = Model.Empty();
                }
                else if (!World.Models.TryGetValue(key, out model))
                {
                    ScriptError error = new(ErrorLevel.Error, $"モデル '{key}' が見つかりません。");
                    World.ErrorCollector.Report(error);

                    model = Model.Empty();
                }

                return KinematicLocatedModelTemplate.CreateKinematicOrNonCollision(World.PhysicsHost, model, pose);
            }).ToArray();
            SplineStructure structure = new(models, (float)from, (float)span, (float)interval, count);
            StructuresKey.Add(structure);
            return structure;
        }

        public SplineStructure PutStructure(IReadOnlyList<string> modelKeys,
            double x, double y, double z, double rotationX, double rotationY, double rotationZ, double from, double span, double interval, int count = int.MaxValue)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return PutStructure(modelKeys, position.ToPose(), from, span, interval, count);
        }

        public SplineStructure PutStructure(IReadOnlyList<string> modelKeys, double x, double y, double z, double from, double span, double interval, int count = int.MaxValue)
        {
            return PutStructure(modelKeys, x, y, z, 0, 0, 0, from, span, interval, count);
        }

        internal ILanePath Build(JunctionFactoryCommand factoryCommand)
        {
            LanePin from = factoryCommand.Junction.Ports[FromPort.Name].Pins[FromPinIndex];
            LanePin to = factoryCommand.Junction.Ports[ToPort.Name].Pins[ToPinIndex];

            ILanePath path;
            if (Curves.Count == 0 && Width.Items.Count == 0)
            {
                path = new BezierLanePath(Key, from, to);
            }
            else
            {
                if (!IsFinalized) BezierToEnd();
                path = new CompositeLanePath(Key, from, to, Curves, Width.Items);
            }

            factoryCommand.Junction.Wire(path);

            foreach (SplineStructure structure in Structures)
            {
                for (int i = 0; i < structure.Count; i++)
                {
                    float s = structure.From + structure.Interval * i;
                    if (path.Length < s) break;

                    LocatedModelTemplate template = structure.Models[i % structure.Models.Count];
                    Pose curvePose = GetSpanPose(s, structure.Span);
                    Pose pose = template.Pose * curvePose;

                    LocatedModelTemplate compiled = KinematicLocatedModelTemplate.CreateKinematicOrNonCollision(World.PhysicsHost, template.Model, pose);
                    factoryCommand.AddStructure(compiled);
                }
            }

            return path;


            Pose GetSpanPose(float s, float span)
            {
                Pose front = path.GetLocalPose(s + span);
                Pose back = path.GetLocalPose(s);
                Vector3 forward = front.Position - back.Position;
                if (forward.LengthSquared() < 1e-6f) return back;

                Vector3 up = Vector3.Normalize(Vector3.Lerp(front.Up, back.Up, 0.5f));

                Vector3 tangent = Vector3.Normalize(forward);
                return Pose.CreateWorldLH(back.Position, tangent, up);
            }
        }

        internal void BuildComponents(ILanePath parent, IErrorCollector errorCollector)
        {
            foreach (ITemplateComponent<ILanePath> component in Components.Values)
            {
                component.Build(parent, errorCollector);
            }
        }


        public class WidthPointList
        {
            private readonly LaneWidth InitialWidth;
            private readonly List<CompositeLanePath.WidthPoint> Points = [];

            private CompositeLanePath.WidthPoint LastItem => Points.Count == 0 ? new(0, InitialWidth) : Points[^1];
            public IReadOnlyList<CompositeLanePath.WidthPoint> Items => Points;

            internal WidthPointList(LaneWidth initialWidth)
            {
                InitialWidth = initialWidth;
            }

            public WidthPointList TransitionTo(LaneWidth width, double length, out double s)
            {
                CompositeLanePath.WidthPoint point = new(LastItem.S + (float)length, width);
                Points.Add(point);
                s = LastItem.S;
                return this;
            }

            public WidthPointList TransitionTo(double left, double right, double length, out double s)
            {
                LaneWidth width = new((float)left, (float)right);
                return TransitionTo(width, length, out s);
            }

            public WidthPointList Constant(double length, out double s) => TransitionTo(LastItem.Width, length, out s);

            public WidthPointList TransitionTo(LaneWidth width, double length) => TransitionTo(width, length, out _);
            public WidthPointList TransitionTo(double left, double right, double length) => TransitionTo(left, right, length, out _);
            public WidthPointList Constant(double length) => Constant(length, out _);
        }
    }
}
