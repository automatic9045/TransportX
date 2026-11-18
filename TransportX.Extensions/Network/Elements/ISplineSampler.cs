using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Extensions.Network.Elements
{
    public interface ISplineSampler
    {
        public static readonly ISplineSampler Empty = new EmptySampler();


        float FromS { get; }
        float ToS { get; }

        IReadOnlyList<float> Sample(SplineBase spline, float sOffset);
        IReadOnlyList<float> Sample(SplineBase spline, float sOffset, float fromS, float toS);


        private class EmptySampler : ISplineSampler
        {
            public float FromS => 0;
            public float ToS => 0;

            public IReadOnlyList<float> Sample(SplineBase spline, float sOffset) => [];
            public IReadOnlyList<float> Sample(SplineBase spline, float sOffset, float fromS, float toS) => [];
        }
    }
}
