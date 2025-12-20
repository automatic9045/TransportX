using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery;

namespace Bus.Common.Extensions.Networks
{
    public class SplineStructure
    {
        public IReadOnlyList<LocatedModel> Models { get; }
        public float From { get; }
        public float Span { get; }
        public float Interval { get; }
        public int Count { get; }

        public SplineStructure(IReadOnlyList<LocatedModel> models, float from, float span, float interval, int count)
        {
            Models = models;
            From = from;
            Span = span;
            Interval = float.Max(0.1f, interval);
            Count = count;
        }
    }
}
