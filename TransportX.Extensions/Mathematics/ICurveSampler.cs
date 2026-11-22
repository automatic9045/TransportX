using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Mathematics;

namespace TransportX.Extensions.Mathematics
{
    public interface ICurveSampler
    {
        public static readonly ICurveSampler Empty = new EmptySampler();


        float FromS { get; }
        float ToS { get; }

        IReadOnlyList<float> Sample(IPoseCurve curve, float sOffset);
        IReadOnlyList<float> Sample(IPoseCurve curve, float sOffset, float fromS, float toS);


        private class EmptySampler : ICurveSampler
        {
            public float FromS => 0;
            public float ToS => 0;

            public IReadOnlyList<float> Sample(IPoseCurve curve, float sOffset) => [];
            public IReadOnlyList<float> Sample(IPoseCurve curve, float sOffset, float fromS, float toS) => [];
        }
    }
}
