using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

using TransportX.Collections;
using TransportX.Components;
using TransportX.Mathematics;
using TransportX.Network;
using TransportX.Physics;
using TransportX.Spatial;

namespace TransportX.Extensions.Network.Elements
{
    public class Junction : NetworkNode
    {
        private static readonly IReadOnlyList<Vector4> DebugColors = [new(0, 0, 1, 1), new(0, 0.75f, 1, 1), new(0, 0.375f, 1, 1), new(0, 0, 0.625f, 1)];
        private static int DebugColorIndex = 0;


        public override IReadOnlyKeyedList<string, NetworkPort> Ports { get; }

        private readonly List<ILanePath> PathsKey = [];
        public override IReadOnlyList<ILanePath> Paths => PathsKey;

        private readonly List<TransformedModel> ModelsKey = [];
        public override IReadOnlyList<TransformedModel> Models => ModelsKey;

        public override IComponentCollection<IComponent> Components { get; } = new ComponentCollection<IComponent>();

        public Junction(WorldPose worldPose, IEnumerable<PortDefinition> ports) : base(worldPose)
        {
            Ports = new KeyedList<string, NetworkPort>(item => item.Name, ports.Select(def => new NetworkPort(def.Name, this, def.Offset, def.Layout)));
        }

        public void Wire(ILanePath path)
        {
            if (!Ports.Contains(path.From.Port)) throw new ArgumentException($"他の {nameof(NetworkElement)} に属しているピンを指定することはできません。", nameof(path));
            if (!Ports.Contains(path.To.Port)) throw new ArgumentException($"他の {nameof(NetworkElement)} に属しているピンを指定することはできません。", nameof(path));

            path.From.Wire(path);
            path.To.Wire(path);
            PathsKey.Add(path);
        }

        public void AddProps(ID3D11Device device, IPhysicsHost physicsHost, IEnumerable<TransformedModelTemplate> props)
        {
            Dictionary<ColliderMaterial, List<StaticTransformedModelTemplate>> templatesToMerge = [];
            foreach (TransformedModelTemplate template in props)
            {
                if (template is StaticTransformedModelTemplate staticTemplate && staticTemplate.CanMerge)
                {
                    if (!templatesToMerge.TryGetValue(staticTemplate.Collider.Material, out List<StaticTransformedModelTemplate>? list))
                    {
                        list = [];
                        templatesToMerge.Add(staticTemplate.Collider.Material, list);
                    }

                    StaticTransformedModelTemplate compiled = new(physicsHost, staticTemplate.Resource, template.Pose * WorldPose.Pose);
                    list.Add(compiled);
                }
                else
                {
                    TransformedModel model = template.Build(pose => pose * WorldPose.Pose);
                    ModelsKey.Add(model);
                }
            }

            foreach (List<StaticTransformedModelTemplate> mergeGroup in templatesToMerge.Values)
            {
                MergedStaticTransformedModel mergedModel = MergedStaticTransformedModel.Create(physicsHost, mergeGroup);
                mergedModel.Collider.CreateDebugModel(device);
                mergedModel.Collider.DebugModel.Color = DebugColors[DebugColorIndex++];
                if (DebugColors.Count <= DebugColorIndex) DebugColorIndex = 0;

                ModelsKey.Add(mergedModel);
            }
        }

        public void PutProps(ID3D11Device device, IPhysicsHost physicsHost, IPoseCurve curve, IEnumerable<SplineProp> props)
        {
            List<TransformedModelTemplate> placedModels = [];
            foreach (SplineProp prop in props)
            {
                IEnumerable<TransformedModelTemplate> templates = prop.Build(physicsHost, curve);
                placedModels.AddRange(templates);
            }

            AddProps(device, physicsHost, placedModels);
        }

        public void PutExtrusions(ID3D11Device device, IPhysicsHost physicsHost, IPoseCurve curve, IEnumerable<SplineExtrusion> extrusions, float sOffset = 0)
        {
            foreach (SplineExtrusion extrusion in extrusions)
            {
                TransformedModel? model = extrusion.Build(device, physicsHost, curve, WorldPose.Pose, sOffset);
                if (model is null) continue;

                if (model is CollidableTransformedModel collidable)
                {
                    collidable.Collider.CreateDebugModel(device);
                    collidable.Collider.DebugModel.Color = DebugColors[DebugColorIndex++];
                    if (DebugColorIndex == DebugColors.Count) DebugColorIndex = 0;
                }

                ModelsKey.Add(model);
            }
        }

        public Pose GetConnectionPose(NetworkPort port, Pose targetPortOffset)
        {
            Pose offsetInv = Pose.Inverse(targetPortOffset);
            Pose pose = offsetInv * Pose.CreateRotationY(-float.Pi) * port.Offset * WorldPose.Pose;
            return pose;
        }

        public T ConnectNew<T>(NetworkPort port, PortDefinition targetPort, Func<WorldPose, T> elementFactory) where T : NetworkElement
        {
            Pose pose = GetConnectionPose(port, targetPort.Offset);

            T element = elementFactory(WorldPose.ChangePose(pose));
            port.ConnectTo(element.Ports[targetPort.Name]);

            return element;
        }
    }
}
