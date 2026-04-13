using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;
using TransportX.Diagnostics;
using TransportX.Network;
using TransportX.Rendering;
using TransportX.Spatial;

using TransportX.Extensions.Network.Elements;
using TransportX.Extensions.Network.Paths;

namespace TransportX.Scripting.Commands
{
    public class JunctionPathFactoryCommand
    {
        private static LanePin EmptyPin(NetworkElement owner) => new(new NetworkPort(string.Empty, owner, Pose.Identity, new LaneLayout()), owner.Ports.Count);
        internal static JunctionPathFactoryCommand Empty(ScriptWorld world, NetworkElement owner)
            => new(world, string.Empty, new StraightLanePath(string.Empty, EmptyPin(owner), EmptyPin(owner)), []);


        private readonly ScriptWorld World;

        public string Key { get; }
        public ILanePath Path { get; }

        private readonly List<SplineStructure> StructuresKey;
        public IReadOnlyList<SplineStructure> Structures => StructuresKey;

        public IComponentCollection<ITemplateComponent<ILanePath>> Components { get; } = new ComponentCollection<ITemplateComponent<ILanePath>>();

        public JunctionPathFactoryCommand(ScriptWorld world, string key, ILanePath path, IEnumerable<SplineStructure> templateStructures)
        {
            World = world;

            Key = key;
            Path = path;
            StructuresKey = [.. templateStructures];
        }

        public SplineStructure PutStructure(IReadOnlyList<string> modelKeys, Pose pose, double from, double span, double interval, int count = int.MaxValue)
        {
            LocatedModelTemplate[] models = modelKeys.Select(key =>
            {
                IModel? model;
                if (key == string.Empty || !World.ModelsKey.GetValue(key, out model))
                {
                    model = Model.Empty();
                }

                return KinematicLocatedModelTemplate.CreateKinematicOrNonCollision(World.PhysicsHost, model, pose);
            }).ToArray();
            SplineStructure structure = new(models, (float)from, (float)span, (float)interval, count);
            StructuresKey.Add(structure);
            return structure;
        }

        public SplineStructure PutStructure(IReadOnlyList<string> modelKeys,
            double x, double y, double z, double rotationX, double rotationY, double rotationZ, double from, double span, double interval, int count = int.MaxValue)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return PutStructure(modelKeys, position.ToPose(), from, span, interval, count);
        }

        public SplineStructure PutStructure(IReadOnlyList<string> modelKeys, double x, double y, double z, double from, double span, double interval, int count = int.MaxValue)
        {
            return PutStructure(modelKeys, x, y, z, 0, 0, 0, from, span, interval, count);
        }

        internal List<LocatedModelTemplate> BuildStructures()
        {
            List<LocatedModelTemplate> structures = [];
            foreach (SplineStructure structure in Structures)
            {
                for (int i = 0; i < structure.Count; i++)
                {
                    float s = structure.From + structure.Interval * i;
                    if (Path.Length < s) break;

                    LocatedModelTemplate template = structure.Models[i % structure.Models.Count];
                    Pose curvePose = GetSpanPose(s, structure.Span);
                    Pose pose = template.Pose * curvePose;

                    LocatedModelTemplate compiled = KinematicLocatedModelTemplate.CreateKinematicOrNonCollision(World.PhysicsHost, template.Model, pose);
                    structures.Add(compiled);
                }
            }

            return structures;


            Pose GetSpanPose(float s, float span)
            {
                Pose front = Path.GetLocalPose(s + span);
                Pose back = Path.GetLocalPose(s);
                Vector3 forward = front.Position - back.Position;
                if (forward.LengthSquared() < 1e-6f) return back;

                Vector3 up = Vector3.Normalize(Vector3.Lerp(front.Up, back.Up, 0.5f));

                Vector3 tangent = Vector3.Normalize(forward);
                return Pose.CreateWorldLH(back.Position, tangent, up);
            }
        }

        internal void BuildComponents(IErrorCollector errorCollector)
        {
            foreach (ITemplateComponent<ILanePath> component in Components.Values)
            {
                component.Build(Path, errorCollector);
            }
        }
    }
}
