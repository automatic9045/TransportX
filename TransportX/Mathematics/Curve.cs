using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Mathematics
{
    public class Curve
    {
        private readonly CurvePoint[] Points;

        public Curve(IReadOnlyCollection<CurvePoint> pointsSorted)
        {
            if (pointsSorted.Count == 0) throw new ArgumentException("リストに要素が含まれていません。", nameof(pointsSorted));

            CurvePoint[] pointsArray = pointsSorted.ToArray();
            for (int i = 1; i < pointsArray.Length; i++)
            {
                if (!(pointsArray[i - 1].X < pointsArray[i].X))
                {
                    throw new ArgumentException("要素が X 座標で昇順にソートされていないか、重複する X 座標が含まれています。", nameof(pointsSorted));
                }
            }

            Points = pointsArray;
        }

        public float GetValue(float x)
        {
            if (x < Points[0].X) return Points[0].Y;

            CurvePoint oldPoint = Points[0];
            for (int i = 0; i < Points.Length; i++)
            {
                CurvePoint point = Points[i];

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
            CurvePoint oldPoint = Points[0];
            if (value == oldPoint.Y)
            {
                x = oldPoint.X;
                return true;
            }

            for (int i = 0; i < Points.Length; i++)
            {
                CurvePoint point = Points[i];

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
