using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Mathematics;
using TransportX.Network;

using TransportX.Extensions.Mathematics;

namespace TransportX.Extensions.Network.Paths
{
    public class CompositeLanePath : LanePath
    {
        private readonly CompositePoseCurve Curve;

        public IReadOnlyList<IPoseCurve> Curves => Curve.Curves;
        public override float Length => Curve.Length;
        public IReadOnlyList<WidthPoint> WidthPoints { get; }

        public CompositeLanePath(string name, LanePin from, LanePin to, IEnumerable<IPoseCurve> curves, IEnumerable<WidthPoint> widthPoints) : base(name, from, to)
        {
            Curve = new CompositePoseCurve(curves);
            WidthPoints = [.. widthPoints];
        }

        protected override Pose GetLocalPoseCore(float s) => Curve.GetPose(s);

        public override LaneWidth GetWidth(float s)
        {
            if (WidthPoints.Count == 0)
            {
                return LaneWidth.Lerp(FromWidth, To.Definition.Width, s / Length);
            }

            float fromS = 0;
            LaneWidth fromWidth = FromWidth;

            float toS = Length;
            LaneWidth toWidth = To.Definition.Width;

            for (int i = 0; i < WidthPoints.Count; i++)
            {
                WidthPoint point = WidthPoints[i];
                if (point.S <= s)
                {
                    fromS = point.S;
                    fromWidth = point.Width;
                }
                else
                {
                    toS = point.S;
                    toWidth = point.Width;
                    break;
                }
            }

            float range = toS - fromS;
            return range <= 1e-3f ? fromWidth : LaneWidth.Lerp(fromWidth, toWidth, (s - fromS) / range);
        }


        public readonly record struct WidthPoint(float S, LaneWidth Width);
    }
}
