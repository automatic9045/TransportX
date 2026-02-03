using System;
using System.Collections.Generic;
using System.Linq;
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
        internal static JunctionPathTemplate Empty(ScriptWorld world) => new(world, string.Empty, 0, string.Empty, 0);


        private readonly ScriptWorld World;

        private readonly string FromPortKey;
        private readonly int FromPinIndex;
        private readonly string ToPortKey;
        private readonly int ToPinIndex;

        private readonly List<Func<Pose, Pose, PoseCurveBase>> CurveFactories = [];
        private readonly List<Pose> IntermediatePoints = [];

        private bool IsFinalized = false;

        public JunctionPathTemplate(ScriptWorld world, string fromPortKey, int fromPinIndex, string toPortKey, int toPinIndex)
        {
            World = world;

            FromPortKey = fromPortKey;
            FromPinIndex = fromPinIndex;
            ToPortKey = toPortKey;
            ToPinIndex = toPinIndex;
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

        private void AddIntermediatePoint(double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            IntermediatePoints.Add(position.ToPose());
        }

        public JunctionPathTemplate StraightTo(double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            if (!CheckNotFinalized()) return this;

            AddIntermediatePoint(x, y, z, rotationX, rotationY, rotationZ);
            CurveFactories.Add((from, to) => new LinearPoseCurve(from, to));

            return this;
        }

        public JunctionPathTemplate BezierTo(double x, double y, double z, double rotationX, double rotationY, double rotationZ, double? controlScale = null)
        {
            if (!CheckNotFinalized()) return this;

            AddIntermediatePoint(x, y, z, rotationX, rotationY, rotationZ);
            CurveFactories.Add((from, to) => new BezierPoseCurve(from, to, (float?)controlScale));

            return this;
        }

        public void StraightToEnd()
        {
            if (!CheckNotFinalized()) return;
            IsFinalized = true;

            CurveFactories.Add((from, to) => new LinearPoseCurve(from, to));
        }

        public void BezierToEnd(double? controlScale = null)
        {
            if (!CheckNotFinalized()) return;
            IsFinalized = true;

            CurveFactories.Add((from, to) => new BezierPoseCurve(from, to, (float?)controlScale));
        }

        internal ILanePath Build(Junction junction)
        {
            LanePin from = junction.Ports[FromPortKey].Pins[FromPinIndex];
            LanePin to = junction.Ports[ToPortKey].Pins[ToPinIndex];

            ILanePath path;
            if (CurveFactories.Count == 0)
            {
                path = new BezierLanePath(from, to);
            }
            else
            {
                PoseCurveBase[] curves = new PoseCurveBase[CurveFactories.Count];
                for (int i = 0; i <= IntermediatePoints.Count; i++)
                {
                    Pose src = i == 0 ? Pose.CreateRotationY(float.Pi) * from.LocalPose : IntermediatePoints[i - 1];
                    Pose dest = i == IntermediatePoints.Count ? to.LocalPose : IntermediatePoints[i];
                    curves[i] = CurveFactories[i](src, dest);
                }

                path = new CompositeLanePath(from, to, curves);
            }

            junction.Wire(path);
            return path;
        }
    }
}
