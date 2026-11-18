using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Extensions.Network.Elements
{
    public class BoundedSplineSampler : ISplineSampler
    {
        public ISplineSampler InnerSampler { get; }

        public float FromS { get; }
        public float ToS { get; }

        private BoundedSplineSampler(ISplineSampler innerSampler, float fromS, float toS)
        {
            while (innerSampler is BoundedSplineSampler boundedSampler)
            {
                innerSampler = boundedSampler.InnerSampler;
            }
            InnerSampler = innerSampler;

            FromS = float.Max(innerSampler.FromS, float.Max(0, fromS));
            ToS = float.Max(FromS, float.Min(innerSampler.ToS, toS));
        }

        public static BoundedSplineSampler Create(ISplineSampler sampler, float fromS, float toS)
        {
            ISplineSampler inner = sampler is BoundedSplineSampler bounded ? bounded.InnerSampler : sampler;
            return new BoundedSplineSampler(inner, fromS, toS);
        }

        public IReadOnlyList<float> Sample(SplineBase spline, float sOffset, float fromS, float toS)
        {
            float effectiveFromS = float.Max(FromS, fromS);
            float effectiveToS = float.Min(ToS, toS);
            if (effectiveToS <= effectiveFromS) return [];

            return InnerSampler.Sample(spline, sOffset, effectiveFromS, effectiveToS);
        }

        public IReadOnlyList<float> Sample(SplineBase spline, float sOffset)
            => InnerSampler.Sample(spline, sOffset, FromS, ToS);
    }
}
