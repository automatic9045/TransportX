using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Sample.Vehicles.Powertrain
{
    internal struct DiagramPoint : IComparable<DiagramPoint>
    {
        public float X { get; }
        public float Y { get; }

        public DiagramPoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        public int CompareTo(DiagramPoint other)
        {
            float diff = X - other.X;
            return float.Sign(diff);
        }

        public override string ToString() => $"{X}, {Y}";
    }
}
