using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Mathematics
{
    public readonly record struct SurfacePoint(float X, float Y, float Z)
    {
        public static implicit operator SurfacePoint((float X, float Y, float Z) point)
        {
            return new SurfacePoint(point.X, point.Y, point.Z);
        }
    }
}
