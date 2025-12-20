using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

using Bus.Common.Physics;
using Bus.Common.Scenery;
using Bus.Common.Scenery.Networks;

namespace Bus.Common.Extensions.Networks
{
    public class SplineFactory : LocatableObject
    {
        private readonly ID3D11Device Device;
        protected readonly IPhysicsHost PhysicsHost;
        protected readonly LaneLayout BaseConnectionLayout;

        protected readonly List<Spline> CreatedSplinesKey = [];
        public IReadOnlyList<Spline> CreatedSplines => CreatedSplinesKey;

        public SplineFactory(ID3D11Device device, IPhysicsHost physicsHost, int plateX, int plateZ, Matrix4x4 transform, LaneLayout baseConnectionLayout)
            : base(plateX, plateZ, transform)
        {
            Device = device;
            PhysicsHost = physicsHost;
            BaseConnectionLayout = baseConnectionLayout;
        }

        public Spline ByCurvature(float curvature, float length)
        {
            Spline spline = new Spline(Device, PhysicsHost, PlateX, PlateZ, Transform, BaseConnectionLayout, curvature, length, CreatedSplines.Count == 0);

            Move(spline.Port.Transition);

            if (0 < CreatedSplines.Count) CreatedSplines[CreatedSplines.Count - 1].SetChild(spline);
            CreatedSplinesKey.Add(spline);
            return spline;
        }

        public Spline Straight(float length)
        {
            return ByCurvature(0, length);
        }

        public Spline ByRadius(float radius, float length)
        {
            return ByCurvature(radius == 0 ? 0 : 1 / radius, length);
        }

        public void PutStructure(SplineStructure structure)
        {
            float from = structure.From;
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
            plateFactory ??= (x, z) => new Plate(x, z);

            foreach (Spline spline in CreatedSplines)
            {
                Plate plate = plates.GetOrAdd(spline.PlateX, spline.PlateZ, plateFactory);
                plate.Network.Add(spline);
            }
        }
    }
}
