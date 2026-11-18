using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Extensions.Network.Elements
{
    public class ExplicitSplineSampler : ISplineSampler
    {
        public IReadOnlyList<float> SSamples { get; }
        public float FromS => SSamples[0];
        public float ToS => SSamples[^1];

        public ExplicitSplineSampler(IReadOnlyList<float> sSamples)
        {
            if (sSamples.Count < 2)
            {
                throw new ArgumentException("サンプリング点は最低 2 点必要です。", nameof(sSamples));
            }

            SSamples = [.. sSamples.Order()];
        }

        public IReadOnlyList<float> Sample(SplineBase spline, float sOffset, float fromS, float toS)
        {
            float effectiveFromS = float.Max(FromS, fromS);
            float effectiveToS = float.Min(ToS, toS);

            float startS = sOffset;
            float endS = sOffset + spline.Length;

            if (endS <= effectiveFromS || effectiveToS <= startS) return [];

            float rangeStart = float.Max(startS, effectiveFromS);
            float rangeEnd = float.Min(endS, effectiveToS);

            List<float> localSamples = [rangeStart - sOffset];

            for (int i = 0; i < SSamples.Count; i++)
            {
                float sGlobal = SSamples[i];
                if (rangeStart < sGlobal && sGlobal < rangeEnd)
                {
                    localSamples.Add(sGlobal - sOffset);
                }
            }

            localSamples.Add(rangeEnd - sOffset);

            List<float> deduplicatedSamples = [];
            for (int i = 0; i < localSamples.Count; i++)
            {
                float sample = localSamples[i];
                if (deduplicatedSamples.Count == 0 || 1e-4f < sample - deduplicatedSamples[^1])
                {
                    deduplicatedSamples.Add(sample);
                }
            }

            return deduplicatedSamples.Count < 2 ? [] : deduplicatedSamples;
        }

        public IReadOnlyList<float> Sample(SplineBase spline, float sOffset)
            => Sample(spline, sOffset, FromS, ToS);
    }
}
