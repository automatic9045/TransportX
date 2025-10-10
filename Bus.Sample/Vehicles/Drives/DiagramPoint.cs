using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Sample.Vehicles.Drives
{
    internal struct DiagramPoint : IComparable<DiagramPoint>
    {
        public double X { get; }
        public double Y { get; }

        public DiagramPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public int CompareTo(DiagramPoint other)
        {
            double diff = X - other.X;
            return double.Sign(diff);
        }

        public override string ToString() => $"{X}, {Y}";
    }
}
