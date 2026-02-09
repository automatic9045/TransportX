using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Network;

using Bus.Common.Extensions.Mathematics;

namespace Bus.Common.Extensions.Network.Paths
{
    public class CompositeLanePath : LanePath
    {
        private readonly IReadOnlyList<float> TotalLengths;

        public IReadOnlyList<PoseCurveBase> Curves { get; }
        public IReadOnlyList<Pose> CurvePoints { get; }
        public override float Length => TotalLengths[^1];
        public IReadOnlyList<WidthPoint> WidthPoints { get; }

        public CompositeLanePath(LanePin from, LanePin to, IEnumerable<PoseCurveBase> curves, IEnumerable<WidthPoint> widthPoints) : base(from, to)
        {
            Curves = curves.ToArray();
            if (Curves.Count == 0) throw new ArgumentException("パスを構成するには少なくとも 1 つの曲線が必要です。", nameof(curves));

            float[] totalLengths = new float[Curves.Count];
            for (int i = 0; i < Curves.Count; i++)
            {
                totalLengths[i] = (i == 0 ? 0 : totalLengths[i - 1]) + Curves[i].Length;
            }
            TotalLengths = totalLengths;

            Pose[] curvePoints = new Pose[int.Max(0, Curves.Count - 1)];
            for (int i = 0; i < Curves.Count - 1; i++)
            {
                curvePoints[i] = Curves[i].To;
            }
            CurvePoints = curvePoints;

            WidthPoints = widthPoints.ToArray();
        }

        public override Pose GetLocalPose(float at)
        {
            if (at <= 0) return Curves[0].From;
            if (Length <= at) return Curves[^1].To;

            for (int i = 0; i < Curves.Count; i++)
            {
                if (at <= TotalLengths[i] || i == Curves.Count - 1)
                {
                    float offset = i == 0 ? 0 : TotalLengths[i - 1];
                    return Curves[i].GetPose(at - offset);
                }
            }

            throw new InvalidOperationException();
        }

        public override LaneWidth GetWidth(float at)
        {
            if (WidthPoints.Count == 0)
            {
                return LaneWidth.Lerp(FromWidth, To.Definition.Width, at / Length);
            }

            float fromS = 0;
            LaneWidth fromWidth = FromWidth;

            float toS = Length;
            LaneWidth toWidth = To.Definition.Width;

            for (int i = 0; i < WidthPoints.Count; i++)
            {
                WidthPoint point = WidthPoints[i];
                if (point.S <= at)
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
            return range <= 1e-3f ? fromWidth : LaneWidth.Lerp(fromWidth, toWidth, (at - fromS) / range);
        }


        public readonly struct WidthPoint
        {
            public float S { get; }
            public LaneWidth Width { get; }

            public WidthPoint(float s, LaneWidth width)
            {
                S = s;
                Width = width;
            }
        }
    }
}
