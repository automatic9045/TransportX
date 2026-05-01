using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Spatial;

namespace TransportX.Extensions.Network.Elements
{
    public class SplineProp
    {
        public IReadOnlyList<TransformedModelTemplate> Models { get; }
        public float From { get; }
        public float Span { get; }
        public float Interval { get; }
        public int Count { get; }

        public SplineProp(IReadOnlyList<TransformedModelTemplate> models, float from, float span, float interval, int count)
        {
            Models = models;
            From = from;
            Span = span;
            Interval = float.Max(0.1f, interval);
            Count = count;
        }
    }
}
