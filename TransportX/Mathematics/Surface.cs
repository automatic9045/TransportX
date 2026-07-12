using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Mathematics
{
    public class Surface
    {
        private readonly float[] XKeys;
        private readonly float[] YKeys;
        private readonly float[,] ZValues;

        public Surface(IReadOnlyCollection<SurfacePoint> points)
        {
            if (points.Count == 0) throw new ArgumentException("リストに要素が含まれていません。", nameof(points));

            float[] xKeys = points.Select(p => p.X).Distinct().OrderBy(x => x).ToArray();
            float[] yKeys = points.Select(p => p.Y).Distinct().OrderBy(y => y).ToArray();

            if (points.Count != xKeys.Length * yKeys.Length)
            {
                throw new ArgumentException("すべての X と Y の組み合わせ（完全な格子状）のデータが揃っていないか、重複する要素が含まれています。", nameof(points));
            }

            XKeys = xKeys;
            YKeys = yKeys;
            ZValues = new float[XKeys.Length, YKeys.Length];

            foreach (SurfacePoint point in points)
            {
                int xIndex = Array.BinarySearch(XKeys, point.X);
                int yIndex = Array.BinarySearch(YKeys, point.Y);
                ZValues[xIndex, yIndex] = point.Z;
            }
        }

        public float GetValue(float x, float y)
        {
            float amountX = GetKeyAmount(XKeys, x, out int xIndex0, out int xIndex1);
            float amountY = GetKeyAmount(YKeys, y, out int yIndex0, out int yIndex1);

            float z00 = ZValues[xIndex0, yIndex0];
            float z10 = ZValues[xIndex1, yIndex0];
            float z01 = ZValues[xIndex0, yIndex1];
            float z11 = ZValues[xIndex1, yIndex1];

            float z0 = float.Lerp(z00, z10, amountX);
            float z1 = float.Lerp(z01, z11, amountX);

            return float.Lerp(z0, z1, amountY);


            static float GetKeyAmount(float[] keys, float value, out int index0, out int index1)
            {
                if (value < keys[0])
                {
                    index0 = 0;
                    index1 = 0;
                    return 0;
                }

                float oldKey = keys[0];
                int oldIndex = 0;

                for (int i = 0; i < keys.Length; i++)
                {
                    float key = keys[i];

                    if (value == key)
                    {
                        index0 = i;
                        index1 = i;
                        return 0;
                    }

                    if (value < key)
                    {
                        index0 = oldIndex;
                        index1 = i;
                        return (value - oldKey) / (key - oldKey);
                    }

                    oldKey = key;
                    oldIndex = i;
                }

                index0 = keys.Length - 1;
                index1 = keys.Length - 1;
                return 0;
            }
        }
    }
}
