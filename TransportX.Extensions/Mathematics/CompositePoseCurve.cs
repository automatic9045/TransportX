using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Mathematics;

namespace TransportX.Extensions.Mathematics
{
    public class CompositePoseCurve : IPoseCurve
    {
        private readonly float[] TotalLengths;

        public IReadOnlyList<IPoseCurve> Curves { get; }
        public float Length => TotalLengths[^1];

        public CompositePoseCurve(IEnumerable<IPoseCurve> curves)
        {
            Curves = [.. curves];
            if (Curves.Count == 0)
            {
                throw new ArgumentException("複合曲線を構成するには少なくとも 1 つの曲線が必要です。", nameof(curves));
            }

            float[] totalLengths = new float[Curves.Count];
            for (int i = 0; i < Curves.Count; i++)
            {
                totalLengths[i] = (i == 0 ? 0 : totalLengths[i - 1]) + Curves[i].Length;
            }
            TotalLengths = totalLengths;
        }

        public Pose GetPose(float s)
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
    }
}
