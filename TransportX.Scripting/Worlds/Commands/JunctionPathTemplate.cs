using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;
using TransportX.Network;

using TransportX.Extensions.Mathematics;
using TransportX.Extensions.Network.Elements;
using TransportX.Extensions.Network.Paths;

namespace TransportX.Scripting.Worlds.Commands
{
    public class JunctionPathTemplate : CurveTemplateBase<JunctionPathTemplate>
    {
        private static PortDefinition EmptyPort() => new(string.Empty, new LaneLayout(), Pose.Identity);
        internal static JunctionPathTemplate Empty(JunctionTemplate parent) => new(parent, string.Empty, EmptyPort(), 0, EmptyPort(), 0);


        private readonly PortDefinition FromPort;
        private readonly int FromPinIndex;
        private readonly PortDefinition ToPort;
        private readonly int ToPinIndex;

        public Pose EndPose { get; }
        public WidthPointList Width { get; }
        public IComponentCollection<ITemplateComponent<ILanePath>> Components { get; } = new ComponentCollection<ITemplateComponent<ILanePath>>();

        public JunctionPathTemplate(JunctionTemplate parent, string key, PortDefinition fromPort, int fromPinIndex, PortDefinition toPort, int toPinIndex)
            : base(parent, key, Pose.CreateRotationY(float.Pi) * fromPort.GetPinLocalPose(fromPinIndex))
        {
            FromPort = fromPort;
            FromPinIndex = fromPinIndex;
            ToPort = toPort;
            ToPinIndex = toPinIndex;

            EndPose = toPort.GetPinLocalPose(toPinIndex);
            Width = new(LaneWidth.Opposition(FromPort.Layout.Lanes[FromPinIndex].Width));
        }

        public JunctionPathTemplate StraightToEnd(out double s)
        {
            StraightTo(EndPose, out s);
            IsFinalized = true;
            return this;
        }

        public JunctionPathTemplate StraightToEnd() => StraightToEnd(out _);

        public JunctionPathTemplate BezierToEnd(out double s, double handleScale = 0.5)
        {
            BezierTo(EndPose, out s, handleScale);
            IsFinalized = true;
            return this;
        }

        public JunctionPathTemplate BezierToEnd(double handleScale = 0.5) => BezierToEnd(out _, handleScale);

        internal JunctionPathFactoryCommand Build(Junction junction)
        {
            LanePin from = junction.Ports[FromPort.Name].Pins[FromPinIndex];
            LanePin to = junction.Ports[ToPort.Name].Pins[ToPinIndex];

            if (!IsFinalized) BezierToEnd();
            IsFinalized = true;

            CompositeLanePath path = new(Key, from, to, Curves, Width.Items);
            junction.Wire(path);

            return new JunctionPathFactoryCommand(Parent.World, Key, path, Props, Extrusions);
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
