using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Mathematics;
using TransportX.Network;

namespace TransportX.Extensions.Network.Paths
{
    public class CompositeLanePath : LanePath
    {
        private readonly IReadOnlyList<float> TotalLengths;

        public IReadOnlyList<IPoseCurve> Curves { get; }
        public IReadOnlyList<Pose> CurvePoints { get; }
        public override float Length => TotalLengths[^1];
        public IReadOnlyList<WidthPoint> WidthPoints { get; }

        public CompositeLanePath(string name, LanePin from, LanePin to, IEnumerable<IPoseCurve> curves, IEnumerable<WidthPoint> widthPoints) : base(name, from, to)
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
                curvePoints[i] = Curves[i].EndPose;
            }
            CurvePoints = curvePoints;

            WidthPoints = widthPoints.ToArray();
        }

        protected override Pose GetLocalPoseCore(float s)
        {
            if (s <= 0) return Curves[0].StartPose;
            if (Length <= s) return Curves[^1].EndPose;

            for (int i = 0; i < Curves.Count; i++)
            {
                if (s <= TotalLengths[i] || i == Curves.Count - 1)
                {
                    float offset = i == 0 ? 0 : TotalLengths[i - 1];
                    return Curves[i].GetPose(s - offset);
                }
            }

            throw new InvalidOperationException();
        }

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
