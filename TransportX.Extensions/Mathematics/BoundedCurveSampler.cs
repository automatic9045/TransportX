using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Mathematics;

namespace TransportX.Extensions.Mathematics
{
    public class BoundedCurveSampler : ICurveSampler
    {
        public ICurveSampler InnerSampler { get; }

        public float FromS { get; }
        public float ToS { get; }

        private BoundedCurveSampler(ICurveSampler innerSampler, float fromS, float toS)
        {
            while (innerSampler is BoundedCurveSampler boundedSampler)
            {
                innerSampler = boundedSampler.InnerSampler;
            }
            InnerSampler = innerSampler;

            FromS = float.Max(innerSampler.FromS, float.Max(0, fromS));
            ToS = float.Max(FromS, float.Min(innerSampler.ToS, toS));
        }

        public static BoundedCurveSampler Create(ICurveSampler sampler, float fromS, float toS)
        {
            ICurveSampler inner = sampler is BoundedCurveSampler bounded ? bounded.InnerSampler : sampler;
            return new BoundedCurveSampler(inner, fromS, toS);
        }

        public IReadOnlyList<float> Sample(IPoseCurve curve, float sOffset, float fromS, float toS)
        {
            float effectiveFromS = float.Max(FromS, fromS);
            float effectiveToS = float.Min(ToS, toS);
            if (effectiveToS <= effectiveFromS) return [];

            return InnerSampler.Sample(curve, sOffset, effectiveFromS, effectiveToS);
        }

        public IReadOnlyList<float> Sample(IPoseCurve curve, float sOffset)
            => InnerSampler.Sample(curve, sOffset, FromS, ToS);
    }
}
