using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Extensions.Utilities
{
    public static class ColorExtensions
    {
        public static Vector4 ToVector4(this Color color)
        {
            return new Vector4(color.R, color.G, color.B, color.A) / 255;
        }

        public static Vector3 ToVector3(this Color color)
        {
            return new Vector3(color.R, color.G, color.B) / 255;
        }
    }
}
