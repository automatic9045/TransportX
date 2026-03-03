using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;
using TransportX.Diagnostics;
using TransportX.Network;

using TransportX.Extensions.Mathematics;
using TransportX.Extensions.Network.Elements;
using TransportX.Extensions.Network.Paths;

namespace TransportX.Scripting.Commands
{
    public class JunctionPathTemplate
    {
        private static PortDefinition EmptyPort() => new PortDefinition(string.Empty, new LaneLayout(), Pose.Identity);
        internal static JunctionPathTemplate Empty(ScriptWorld world) => new(world, string.Empty, EmptyPort(), 0, EmptyPort(), 0);


        private readonly ScriptWorld World;

        private readonly string Key;
        private readonly PortDefinition FromPort;
        private readonly int FromPinIndex;
        private readonly PortDefinition ToPort;
        private readonly int ToPinIndex;

        private readonly List<PoseCurveBase> Curves = [];

        private float Length = 0;
        private bool IsFinalized = false;

        private Pose LastCurvePoint => Curves.Count == 0 ? FromPort.GetPinLocalPose(FromPinIndex) : Curves[^1].To;
        public WidthPointList Width { get; }

        public IComponentCollection<ITemplateComponent<ILanePath>> Components { get; } = new ComponentCollection<ITemplateComponent<ILanePath>>();

        public JunctionPathTemplate(ScriptWorld world, string key, PortDefinition fromPort, int fromPinIndex, PortDefinition toPort, int toPinIndex)
        {
            World = world;

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

        internal ILanePath Build(Junction junction)
        {
            LanePin from = junction.Ports[FromPort.Name].Pins[FromPinIndex];
            LanePin to = junction.Ports[ToPort.Name].Pins[ToPinIndex];

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

            junction.Wire(path);
            return path;
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
