using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Sample.Vehicles.Powertrain
{
    internal class Diagram
    {
        private readonly List<DiagramPoint> Points;

        public Diagram(IEnumerable<DiagramPoint> points)
        {
            Points = points.ToList();
            Points.Sort();
        }

        public float GetValue(float x)
        {
            DiagramPoint oldPoint = new DiagramPoint(float.MinValue, 0);
            foreach (DiagramPoint point in Points)
            {
                if (x == point.X)
                {
                    return point.Y;
                }

                if (x < point.X)
                {
                    return oldPoint.Y + (point.Y - oldPoint.Y) * (x - oldPoint.X) / (point.X - oldPoint.X);
                }

                oldPoint = point;
            }

            return oldPoint.Y;
        }

        public bool TryGetX(float value, out float x)
        {
            DiagramPoint oldPoint = default;
            foreach (DiagramPoint point in Points)
            {
                if (value == point.Y)
                {
                    x = point.X;
                    return true;
                }

                if (float.Sign(oldPoint.Y - value) == float.Sign(value - point.Y))
                {
                    x = oldPoint.X + (point.X - oldPoint.X) * (value - oldPoint.Y) / (point.Y - oldPoint.Y);
                    return true;
                }

                oldPoint = point;
            }

            x = default;
            return false;
        }
    }
}
