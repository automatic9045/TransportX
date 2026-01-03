using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Extensions.Networks
{
    public class CubicBezier3D
    {
        private const int ArcLengthSamples = 16;


        private readonly Vector3 P0, P1, P2, P3;
        private readonly float[] CumulativeLengths;

        public float TotalLength { get; }

        public CubicBezier3D(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            P0 = p0;
            P1 = p1;
            P2 = p2;
            P3 = p3;

            CumulativeLengths = new float[ArcLengthSamples + 1];
            TotalLength = 0;
            CumulativeLengths[0] = 0;

            Vector3 prevPosition = P0;
            for (int i = 1; i <= ArcLengthSamples; i++)
            {
                float t = (float)i / ArcLengthSamples;
                Vector3 position = GetPoint(t);
                TotalLength += Vector3.Distance(prevPosition, position);
                CumulativeLengths[i] = TotalLength;
                prevPosition = position;
            }
        }

        public Vector3 GetPoint(float t)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;
            return uuu * P0 + 3 * uu * t * P1 + 3 * u * tt * P2 + ttt * P3;
        }

        public Vector3 GetTangent(float t)
        {
            float u = 1 - t;
            Vector3 tangent = 3 * u * u * (P1 - P0) + 6 * u * t * (P2 - P1) + 3 * t * t * (P3 - P2);
            return 1e-6f < tangent.LengthSquared() ? Vector3.Normalize(tangent) : Vector3.UnitZ;
        }

        public float GetT(float s)
        {
            if (s <= 0) return 0;
            if (TotalLength <= s) return 1;

            int index = 0;
            for (int i = 1; i < CumulativeLengths.Length; i++)
            {
                if (s <= CumulativeLengths[i])
                {
                    index = i - 1;
                    break;
                }
            }

            float lengthStart = CumulativeLengths[index];
            float lengthEnd = CumulativeLengths[index + 1];
            float segmentFraction = (s - lengthStart) / (lengthEnd - lengthStart);

            float tStep = 1f / ArcLengthSamples;
            return (index + segmentFraction) * tStep;
        }
    }
}
