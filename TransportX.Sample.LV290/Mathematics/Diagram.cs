using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Sample.LV290.Mathematics
{
    internal class Diagram
    {
        private readonly DiagramPoint[] Points;

        public Diagram(IReadOnlyList<DiagramPoint> pointsSorted)
        {
            if (pointsSorted.Count == 0) throw new ArgumentException("リストに要素が含まれていません。", nameof(pointsSorted));

            Points = pointsSorted.ToArray();
        }

        public float GetValue(float x)
        {
            if (x < Points[0].X) return Points[0].Y;

            DiagramPoint oldPoint = Points[0];
            for (int i = 0; i < Points.Length; i++)
            {
                DiagramPoint point = Points[i];

                if (x == point.X)
                {
                    return point.Y;
                }

                if (x < point.X)
                {
                    float amount = (x - oldPoint.X) / (point.X - oldPoint.X);
                    return float.Lerp(oldPoint.Y, point.Y, amount);
                }

                oldPoint = point;
            }

            return oldPoint.Y;
        }

        public bool TryGetX(float value, out float x)
        {
            DiagramPoint oldPoint = Points[0];
            if (value == oldPoint.Y)
            {
                x = oldPoint.X;
                return true;
            }

            for (int i = 0; i < Points.Length; i++)
            {
                DiagramPoint point = Points[i];

                if (value == point.Y)
                {
                    x = point.X;
                    return true;
                }

                if ((oldPoint.Y < value && value < point.Y) || (point.Y < value && value < oldPoint.Y))
                {
                    float amount = (value - oldPoint.Y) / (point.Y - oldPoint.Y);
                    x = float.Lerp(oldPoint.X, point.X, amount);
                    return true;
                }

                oldPoint = point;
            }

            x = default;
            return false;
        }
    }
}
