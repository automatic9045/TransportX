using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Scenery.Networks
{
    public class SplineStructure
    {
        public IReadOnlyList<LocatedModel> Models { get; }
        public double From { get; }
        public double Span { get; }
        public double Interval { get; }
        public int Count { get; }

        public SplineStructure(IReadOnlyList<LocatedModel> models, double from, double span, double interval, int count)
        {
            Models = models;
            From = from;
            Span = span;
            Interval = double.Max(0.1, interval);
            Count = count;
        }
    }
}
