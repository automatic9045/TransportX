using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;

using Bus.Common.Rendering;

namespace Bus.Common.Scenery.Networks
{
    public class Spline : NetworkEdge
    {
        private readonly Simulation Simulation;

        public override LaneConnector Port { get; }
        public override ElementPath Path { get; }

        public double Curvature { get; }
        public double Length { get; }

        private readonly List<LocatedModel> CompiledModels = new List<LocatedModel>();
        public override IReadOnlyList<LocatedModel> Models => CompiledModels;

        public Spline(Simulation simulation, int plateX, int plateZ, Matrix4x4 locator, LaneConnector pairedPort, double curvature, double length, bool isRoot)
            : base(plateX, plateZ, locator, isRoot)
        {
            Simulation = simulation;

            Curvature = curvature;
            Length = length;

            Port = pairedPort.CreateOpposition();
            Path = new ElementPath(GetTransform(Length), Port.CreateOpposition());
        }

        public void AddStructure(SplineStructure structure)
        {
            Matrix4x4 span = Curvature == 0 || structure.Span == 0 ? Matrix4x4.Identity : Matrix4x4.CreateRotationY((float)(structure.Span * Curvature / 2));

            Matrix4x4 world = GetTransform(structure.From) * Locator;
            for (int i = 0; i < structure.Count; i++)
            {
                LocatedModel source = structure.Models[i % structure.Models.Count];
                Matrix4x4 locator = source.InitialLocator * span * world;

                LocatedModel compiled = LocatedModel.CreateStaticOrNonCollision(Simulation, source.Model, locator);
                CompiledModels.Add(compiled);

                world = GetTransform(structure.Interval) * world;
            }
        }

        public void AddStructures(IEnumerable<SplineStructure> structures)
        {
            foreach (SplineStructure structure in structures)
            {
                AddStructure(structure);
            }
        }

        public Matrix4x4 GetTransform(double at)
        {
            double angle = at * Curvature;
            double x = Curvature == 0 ? 0 : (1 - double.Cos(angle)) / Curvature;
            double z = Curvature == 0 ? at : double.Sin(angle) / Curvature;
            return Matrix4x4.CreateRotationY((float)angle) * Matrix4x4.CreateTranslation((float)x, 0, (float)z);
        }
    }
}
