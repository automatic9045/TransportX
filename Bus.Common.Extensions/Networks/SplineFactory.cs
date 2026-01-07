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

        protected readonly List<SplineStructure> Structures = [];

        public LaneLayout OutletLayout { get; }
        public NetworkPort? SourcePort { get; }

        protected readonly List<SplineBase> CreatedSplinesKey = [];
        public IReadOnlyList<SplineBase> CreatedSplines => CreatedSplinesKey;

        public SplineFactory(ID3D11Device device, IPhysicsHost physicsHost, int plateX, int plateZ, Matrix4x4 transform, LaneLayout outletLayout, NetworkPort? sourcePort)
            : base(plateX, plateZ, transform)
        {
            Device = device;
            PhysicsHost = physicsHost;
            OutletLayout = outletLayout;

            if (sourcePort is not null && sourcePort.Layout != OutletLayout) throw new ArgumentException("進路の接続部形状が一致しません。", nameof(sourcePort));
            SourcePort = sourcePort;
        }

        protected bool ApplyStructureToSpline(int structureIndex, SplineBase spline)
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

        internal void SetNext(SplineBase spline)
        {
            List<int> indicesToRemove = [];
            for (int i = 0; i < Structures.Count; i++) // 登録済のストラクチャーをこのスプラインに設置
            {
                if (!ApplyStructureToSpline(i, spline)) indicesToRemove.Add(i);
            }
            foreach (int i in indicesToRemove) Structures.RemoveAt(i);

            Move(spline.Outlet.Offset);

            if (CreatedSplines.Count == 0)
            {
                SourcePort?.ConnectTo(spline.Inlet);
            }
            else
            {
                CreatedSplines[CreatedSplines.Count - 1].SetChild(spline);
            }

            CreatedSplinesKey.Add(spline);
        }

        public Spline ByCurvature(float curvature, float length)
        {
            Spline spline = new Spline(Device, PhysicsHost, PlateX, PlateZ, Transform, OutletLayout, curvature, length);
            SetNext(spline);
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

        public BezierSpline InterpolateByBezier(NetworkPort targetPort, float handleScale = 0.5f)
        {
            Matrix4x4 from = Transform;
            PlateOffset plateOffset = GetPlateOffset(targetPort.Owner);
            Matrix4x4 to = targetPort.Offset * targetPort.Owner.Transform * plateOffset.Transform;

            BezierSpline spline = new(Device, PhysicsHost, PlateX, PlateZ, from, to, OutletLayout, handleScale);
            SetNext(spline);
            spline.Outlet.ConnectTo(targetPort);
            return spline;
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

        public T ConnectNew<T>(PortDefinition targetPort, Func<int, int, Matrix4x4, T> elementFactory) where T : NetworkElement
        {
            Matrix4x4.Invert(targetPort.Offset, out Matrix4x4 offsetInv);
            Matrix4x4 transform = offsetInv * Matrix4x4.CreateRotationY(-float.Pi) * Transform;

            T element = elementFactory(PlateX, PlateZ, transform);
            CreatedSplines[CreatedSplines.Count - 1].Outlet.ConnectTo(element.Ports[targetPort.Name]);

            return element;
        }
    }
}
