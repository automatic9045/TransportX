using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Rendering
{
    public static class ColorExtensions
    {
        public static Vector4 ToLinear(this Vector4 srgbColor)
        {
            return new Vector4(float.Pow(srgbColor.X, 2.2f), float.Pow(srgbColor.Y, 2.2f), float.Pow(srgbColor.Z, 2.2f), srgbColor.W);
        }

        public static Vector4 ToSrgb(this Vector4 linearColor)
        {
            float power = 1.0f / 2.2f;
            return new Vector4(float.Pow(linearColor.X, power), float.Pow(linearColor.Y, power), float.Pow(linearColor.Z, power), linearColor.W);
        }
    }
}
