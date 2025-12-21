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
        protected readonly ID3D11Device Device;
        protected readonly IPhysicsHost PhysicsHost;
        protected readonly LaneLayout BaseConnectionLayout;

        protected readonly List<SplineStructure> Structures = [];

        protected readonly List<Spline> CreatedSplinesKey = [];
        public IReadOnlyList<Spline> CreatedSplines => CreatedSplinesKey;

        public SplineFactory(ID3D11Device device, IPhysicsHost physicsHost, int plateX, int plateZ, Matrix4x4 transform, LaneLayout baseConnectionLayout)
            : base(plateX, plateZ, transform)
        {
            Device = device;
            PhysicsHost = physicsHost;
            BaseConnectionLayout = baseConnectionLayout;
        }

        protected bool ApplyStructureToSpline(int structureIndex, Spline spline)
        {
            SplineStructure structure = Structures[structureIndex];
            int count = int.Min((int)float.Ceiling((spline.Length - structure.From) / structure.Interval), structure.Count);

            SplineStructure splittedStructure = new(structure.Models, structure.From, structure.Span, structure.Interval, count);
            spline.AddStructure(splittedStructure);

            if (count == structure.Count)
            {
                return false;
            }
            else
            {
                LocatedModelTemplate[] nextModels = new LocatedModelTemplate[structure.Models.Count];
                for (int i = 0; i < nextModels.Length; i++)
                {
                    nextModels[i] = structure.Models[(i + count) % nextModels.Length];
                }

                float nextFrom = structure.From + structure.Interval * count - spline.Length;
                int nextCount = structure.Count - count;

                Structures[structureIndex] = new SplineStructure(nextModels, nextFrom, structure.Span, structure.Interval, nextCount);
                return true;
            }
        }

        public Spline ByCurvature(float curvature, float length)
        {
            Spline spline = new Spline(Device, PhysicsHost, PlateX, PlateZ, Transform, BaseConnectionLayout, curvature, length, CreatedSplines.Count == 0);

            List<int> indicesToRemove = [];
            for (int i = 0; i < Structures.Count; i++) // 登録済のストラクチャーをこのスプラインに設置
            {
                if (!ApplyStructureToSpline(i, spline)) indicesToRemove.Add(i);
            }
            foreach (int i in indicesToRemove) Structures.RemoveAt(i);

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
            int index = Structures.Count;
            Structures.Add(structure);
            foreach (Spline spline in CreatedSplines) // このストラクチャーを敷設済のスプラインに設置
            {
                if (!ApplyStructureToSpline(index, spline))
                {
                    Structures.RemoveAt(index);
                    break;
                }
            }
        }

        public void PutStructures(IEnumerable<SplineStructure> structures)
        {
            foreach (SplineStructure structure in structures)
            {
                PutStructure(structure);
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
