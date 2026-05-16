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

namespace TransportX.Scripting.Worlds.Commands
{
    public class JunctionPathFactoryCommand
    {
        private static LanePin EmptyPin(NetworkElement owner) => new(new NetworkPort(string.Empty, owner, Pose.Identity, new LaneLayout()), owner.Ports.Count);
        internal static JunctionPathFactoryCommand Empty(ScriptWorld world, NetworkElement owner)
            => new(world, string.Empty, new StraightLanePath(string.Empty, EmptyPin(owner), EmptyPin(owner)), []);


        private readonly ScriptWorld World;

        public string Key { get; }
        public ILanePath Path { get; }

        private readonly List<SplineProp> PropsKey;
        public IReadOnlyList<SplineProp> Props => PropsKey;

        public IComponentCollection<ITemplateComponent<ILanePath>> Components { get; } = new ComponentCollection<ITemplateComponent<ILanePath>>();

        public JunctionPathFactoryCommand(ScriptWorld world, string key, ILanePath path, IEnumerable<SplineProp> templateProps)
        {
            World = world;

            Key = key;
            Path = path;
            PropsKey = [.. templateProps];
        }

        public SplineProp PutProp(IReadOnlyList<string> modelKeys, Pose pose, double from, double span, double interval, int count = int.MaxValue)
        {
            TransformedModelTemplate[] models = modelKeys.Select(key =>
            {
                IModel? model;
                if (key == string.Empty || !World.ModelsKey.GetValue(key, out model))
                {
                    model = Model.Empty();
                }

                return KinematicTransformedModelTemplate.CreateKinematicOrNonCollision(World.PhysicsHost, model, pose);
            }).ToArray();
            SplineProp prop = new(models, (float)from, (float)span, (float)interval, count);
            PropsKey.Add(prop);
            return prop;
        }

        public SplineProp PutProp(IReadOnlyList<string> modelKeys,
            double x, double y, double z, double rotationX, double rotationY, double rotationZ, double from, double span, double interval, int count = int.MaxValue)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return PutProp(modelKeys, position.ToPose(), from, span, interval, count);
        }

        public SplineProp PutProp(IReadOnlyList<string> modelKeys, double x, double y, double z, double from, double span, double interval, int count = int.MaxValue)
        {
            return PutProp(modelKeys, x, y, z, 0, 0, 0, from, span, interval, count);
        }

        internal List<TransformedModelTemplate> BuildProps()
        {
            List<TransformedModelTemplate> props = [];
            foreach (SplineProp prop in Props)
            {
                for (int i = 0; i < prop.Count; i++)
                {
                    float s = prop.From + prop.Interval * i;
                    if (Path.Length < s) break;

                    TransformedModelTemplate template = prop.Models[i % prop.Models.Count];
                    Pose curvePose = GetSpanPose(s, prop.Span);
                    Pose pose = template.Pose * curvePose;

                    TransformedModelTemplate compiled = KinematicTransformedModelTemplate.CreateKinematicOrNonCollision(World.PhysicsHost, template.Model, pose);
                    props.Add(compiled);
                }
            }

            return props;


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
