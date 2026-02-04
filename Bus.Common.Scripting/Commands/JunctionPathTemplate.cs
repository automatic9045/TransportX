using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Diagnostics;
using Bus.Common.Scenery.Networks;

using Bus.Common.Extensions.Mathematics;
using Bus.Common.Extensions.Networks.Elements;
using Bus.Common.Extensions.Networks.Paths;

namespace Bus.Common.Scripting.Commands
{
    public class JunctionPathTemplate
    {
        private static PortDefinition EmptyPort() => new PortDefinition(string.Empty, new LaneLayout(), Pose.Identity);
        internal static JunctionPathTemplate Empty(ScriptWorld world) => new(world, EmptyPort(), 0, EmptyPort(), 0);


        private readonly ScriptWorld World;

        private readonly PortDefinition FromPort;
        private readonly int FromPinIndex;
        private readonly PortDefinition ToPort;
        private readonly int ToPinIndex;

        private readonly List<PoseCurveBase> Curves = [];

        private bool IsFinalized = false;

        private Pose LastCurvePoint => Curves.Count == 0 ? FromPort.GetPinLocalPose(FromPinIndex) : Curves[^1].To;
        public WidthPointList Width { get; }

        public JunctionPathTemplate(ScriptWorld world, PortDefinition fromPort, int fromPinIndex, PortDefinition toPort, int toPinIndex)
        {
            World = world;

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

        public JunctionPathTemplate StraightTo(double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            if (!CheckNotFinalized()) return this;

            Pose to = CreateCurvePoint(x, y, z, rotationX, rotationY, rotationZ);
            Curves.Add(new LinearPoseCurve(LastCurvePoint, to));

            return this;
        }

        public JunctionPathTemplate BezierTo(double x, double y, double z, double rotationX, double rotationY, double rotationZ, double? controlScale = null)
        {
            if (!CheckNotFinalized()) return this;

            Pose to = CreateCurvePoint(x, y, z, rotationX, rotationY, rotationZ);
            Curves.Add(new BezierPoseCurve(LastCurvePoint, to, (float?)controlScale));

            return this;
        }

        public void StraightToEnd()
        {
            if (!CheckNotFinalized()) return;
            IsFinalized = true;

            Pose to = ToPort.GetPinLocalPose(ToPinIndex);
            Curves.Add(new LinearPoseCurve(LastCurvePoint, to));
        }

        public void BezierToEnd(double? controlScale = null)
        {
            if (!CheckNotFinalized()) return;
            IsFinalized = true;

            Pose to = ToPort.GetPinLocalPose(ToPinIndex);
            Curves.Add(new BezierPoseCurve(LastCurvePoint, to, (float?)controlScale));
        }

        internal ILanePath Build(Junction junction)
        {
            LanePin from = junction.Ports[FromPort.Name].Pins[FromPinIndex];
            LanePin to = junction.Ports[ToPort.Name].Pins[ToPinIndex];

            ILanePath path;
            if (Curves.Count == 0 && Width.Items.Count == 0)
            {
                path = new BezierLanePath(from, to);
            }
            else
            {
                if (!IsFinalized) BezierToEnd();
                path = new CompositeLanePath(from, to, Curves, Width.Items);
            }

            junction.Wire(path);
            return path;
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

            public WidthPointList TransitionTo(LaneWidth width, double length)
            {
                CompositeLanePath.WidthPoint point = new(LastItem.S + (float)length, width);
                Points.Add(point);
                return this;
            }

            public WidthPointList TransitionTo(double left, double right, double length)
            {
                LaneWidth width = new((float)left, (float)right);
                return TransitionTo(width, length);
            }

            public WidthPointList Constant(double length)
            {
                return TransitionTo(LastItem.Width, length);
            }
        }
    }
}
