using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Scenery.Networks
{
    public class SplineFactory : LocatableObject
    {
        protected readonly LaneConnector BasePairedPort;

        protected readonly List<Spline> CreatedSplinesKey = new List<Spline>();
        public IReadOnlyList<Spline> CreatedSplines => CreatedSplinesKey;

        public SplineFactory(int plateX, int plateZ, Matrix4x4 locator, LaneConnector basePairedPort) : base(plateX, plateZ, locator)
        {
            BasePairedPort = basePairedPort;
        }

        public Spline ByCurvature(double curvature, double length)
        {
            Spline spline = new Spline(PlateX, PlateZ, Locator, BasePairedPort, curvature, length, CreatedSplines.Count == 0);

            Move(spline.Path.Transition);

            if (0 < CreatedSplines.Count) CreatedSplines[CreatedSplines.Count - 1].SetChild(spline);
            CreatedSplinesKey.Add(spline);
            return spline;
        }

        public Spline Straight(double length)
        {
            return ByCurvature(0, length);
        }

        public Spline ByRadius(double radius, double length)
        {
            return ByCurvature(radius == 0 ? 0 : 1 / radius, length);
        }

        public void PutStructure(SplineStructure structure)
        {
            double from = structure.From;
            int totalCount = 0;
            foreach (Spline spline in CreatedSplines)
            {
                if (spline.Length <= from)
                {
                    from -= spline.Length;
                    continue;
                }

                int count = (int)((spline.Length - from) / structure.Interval);
                if (structure.Count < totalCount + count) count = structure.Count - totalCount;

                LocatedModel[] models = new LocatedModel[structure.Models.Count];
                for (int i = 0; i < models.Length; i++)
                {
                    models[i] = structure.Models[(i + totalCount) % models.Length];
                }

                SplineStructure splittedStructure = new SplineStructure(models, from, structure.Span, structure.Interval, count);
                spline.AddStructure(splittedStructure);

                totalCount += count;
                if (structure.Count <= totalCount) break;

                from += structure.Interval * count - spline.Length;
            }
        }

        public void AddSplinesToPlates(PlateCollection plates, Func<int, int, Plate>? plateFactory = null)
        {
            plateFactory ??= (x, z) => new Plate();

            foreach (Spline spline in CreatedSplines)
            {
                Plate plate = plates.GetOrAdd(spline.PlateX, spline.PlateZ, plateFactory);
                plate.Network.Add(spline);
            }
        }
    }
}
