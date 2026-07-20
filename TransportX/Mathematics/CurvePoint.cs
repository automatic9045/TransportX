using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Mathematics
{
    public readonly record struct CurvePoint(float X, float Y)
    {
        public static implicit operator CurvePoint((float X, float Y) point)
        {
            return new CurvePoint(point.X, point.Y);
        }

        public static implicit operator CurvePoint((double X, double Y) point)
        {
            return new CurvePoint((float)point.X, (float)point.Y);
        }
    }
}
