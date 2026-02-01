using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery.Networks;

using Bus.Common.Extensions.Mathematics;

namespace Bus.Common.Extensions.Networks.Paths
{
    public class CompositeLanePath : LanePath
    {
        private readonly IReadOnlyList<float> TotalLengths;

        public IReadOnlyList<PoseCurveBase> Curves { get; }
        public IReadOnlyList<Pose> IntermediatePoints { get; }
        public override float Length => TotalLengths[^1];

        public CompositeLanePath(LanePin from, LanePin to, IEnumerable<PoseCurveBase> curves) : base(from, to)
        {
            Curves = curves.ToArray();
            if (Curves.Count == 0) throw new ArgumentException("パスを構成するには少なくとも 1 つの曲線が必要です。", nameof(curves));

            float[] totalLengths = new float[Curves.Count];
            for (int i = 0; i < Curves.Count; i++)
            {
                totalLengths[i] = (i == 0 ? 0 : totalLengths[i - 1]) + Curves[i].Length;
            }
            TotalLengths = totalLengths;

            Pose[] intermediatePoints = new Pose[int.Max(0, Curves.Count - 1)];
            for (int i = 0; i < Curves.Count - 1; i++)
            {
                intermediatePoints[i] = Curves[i].To;
            }
            IntermediatePoints = intermediatePoints;
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
    }
}
