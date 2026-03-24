using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Sample.Mathematics
{
    internal readonly struct DiagramPoint : IComparable<DiagramPoint>
    {
        public float X { get; }
        public float Y { get; }

        public DiagramPoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        public int CompareTo(DiagramPoint other) => X.CompareTo(other.X);
        public override string ToString() => $"{X}, {Y}";
    }
}
