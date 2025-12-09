using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Physics;

namespace Bus.Common.Scenery.Networks
{
    public class Spline : NetworkEdge
    {
        private readonly IPhysicsHost PhysicsHost;

        public override LaneConnector Port { get; }
        public override ElementPath Path { get; }

        public float Curvature { get; }
        public float Length { get; }

        private readonly List<LocatedModel> CompiledModels = new List<LocatedModel>();
        public override IReadOnlyList<LocatedModel> Models => CompiledModels;

        public Spline(IPhysicsHost physicsHost, int plateX, int plateZ, Matrix4x4 transform, LaneConnector pairedPort, float curvature, float length, bool isRoot)
            : base(plateX, plateZ, transform, isRoot)
        {
            PhysicsHost = physicsHost;

            Curvature = curvature;
            Length = length;

            Port = pairedPort.CreateOpposition();
            Path = new ElementPath(GetTransform(Length), Port.CreateOpposition());
        }

        public void AddStructure(SplineStructure structure)
        {
            Matrix4x4 span = Curvature == 0 || structure.Span == 0 ? Matrix4x4.Identity : Matrix4x4.CreateRotationY(structure.Span * Curvature / 2);

            List<KinematicLocatedModel> modelsToMerge = [];
            Matrix4x4 world = GetTransform(structure.From) * Transform;
            for (int i = 0; i < structure.Count; i++)
            {
                LocatedModel source = structure.Models[i % structure.Models.Count];
                Matrix4x4 transform = source.BaseTransform * span * world;

                LocatedModel compiled = KinematicLocatedModel.CreateKinematicOrNonCollision(PhysicsHost, source.Model, transform);
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

        public Matrix4x4 GetTransform(float at)
        {
            float angle = at * Curvature;
            float x = Curvature == 0 ? 0 : (1 - float.Cos(angle)) / Curvature;
            float z = Curvature == 0 ? at : float.Sin(angle) / Curvature;
            return Matrix4x4.CreateRotationY(angle) * Matrix4x4.CreateTranslation(x, 0, z);
        }
    }
}
