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
    public class Spline : NetworkEdge
    {
        private readonly ID3D11Device Device;
        private readonly IPhysicsHost PhysicsHost;

        public override NetworkPort Inlet { get; }
        public override NetworkPort Outlet { get; }

        private readonly List<LanePath> PathsKey = [];
        public override IReadOnlyList<LanePath> Paths => PathsKey;

        private readonly List<LocatedModel> ModelsKey = [];
        public override IReadOnlyList<LocatedModel> Models => ModelsKey;

        public float Curvature { get; }
        public float Length { get; }

        public Spline(ID3D11Device device, IPhysicsHost physicsHost,
            int plateX, int plateZ, Matrix4x4 transform, LaneLayout outletLayout, float curvature, float length)
            : base(plateX, plateZ, transform)
        {
            Device = device;
            PhysicsHost = physicsHost;

            Curvature = curvature;
            Length = length;

            Inlet = new NetworkPort(nameof(Inlet), this, Matrix4x4.CreateRotationY(float.Pi), outletLayout.Opposition);
            Outlet = new NetworkPort(nameof(Outlet), this, GetTransform(Length), outletLayout);

            for (int i = 0; i < Inlet.Layout.Lanes.Count; i++)
            {
                LanePin inlet = Inlet.Pins[i];
                LanePin outlet = Outlet.Pins[Inlet.Layout.Lanes.Count - 1 - i];

                SplineLanePath path = new(inlet, outlet);
                inlet.Wire(path);
                outlet.Wire(path);
                PathsKey.Add(path);
            }
        }

        public void AddStructure(SplineStructure structure)
        {
            Matrix4x4 span = Curvature == 0 || structure.Span == 0 ? Matrix4x4.Identity : Matrix4x4.CreateRotationY(structure.Span * Curvature / 2);

            List<KinematicLocatedModelTemplate> modelsToMerge = [];
            Matrix4x4 world = GetTransform(structure.From) * Transform;
            for (int i = 0; i < structure.Count; i++)
            {
                LocatedModelTemplate source = structure.Models[i % structure.Models.Count];
                Matrix4x4 transform = source.Transform * span * world;

                LocatedModelTemplate compiled = KinematicLocatedModelTemplate.CreateKinematicOrNonCollision(PhysicsHost, source.Model, transform);
                if (compiled is KinematicLocatedModelTemplate compiledDynamic)
                {
                    modelsToMerge.Add(compiledDynamic);
                }
                else
                {
                    LocatedModel model = compiled.Build();
                    ModelsKey.Add(model);
                }

                world = GetTransform(structure.Interval) * world;
            }

            if (0 < modelsToMerge.Count)
            {
                MergedKinematicLocatedModel mergedModel = MergedKinematicLocatedModel.Create(PhysicsHost, modelsToMerge);
                mergedModel.Model.Collider.CreateDebugModel(Device);
                mergedModel.Model.Collider.DebugModelColor = new Vector4(0, 0, 1, 1);
                ModelsKey.Add(mergedModel);
            }
        }

        public void AddStructures(IEnumerable<SplineStructure> structures)
        {
            foreach (SplineStructure structure in structures)
            {
                AddStructure(structure);
            }
        }

        public Matrix4x4 GetTransform(float at)
        {
            float angle = at * Curvature;
            float x = Curvature == 0 ? 0 : (1 - float.Cos(angle)) / Curvature;
            float z = Curvature == 0 ? at : float.Sin(angle) / Curvature;
            return Matrix4x4.CreateRotationY(angle) * Matrix4x4.CreateTranslation(x, 0, z);
        }
    }
}
