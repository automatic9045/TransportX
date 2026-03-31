using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

using TransportX.Collections;
using TransportX.Components;
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

        private readonly List<LocatedModel> ModelsKey = [];
        public override IReadOnlyList<LocatedModel> Models => ModelsKey;

        public override IComponentCollection<IComponent> Components { get; } = new ComponentCollection<IComponent>();

        public Junction(int plateX, int plateZ, Pose pose, IEnumerable<PortDefinition> ports) : base(plateX, plateZ, pose)
        {
            Ports = new KeyedList<string, NetworkPort>(item => item.Name, ports.Select(def => new NetworkPort(def.Name, this, def.Offset, def.Layout)));
        }

        public ILanePath Wire(ILanePath path)
        {
            if (!Ports.Contains(path.From.Port)) throw new ArgumentException($"他の {nameof(NetworkElement)} に属しているピンを指定することはできません。", nameof(path));
            if (!Ports.Contains(path.To.Port)) throw new ArgumentException($"他の {nameof(NetworkElement)} に属しているピンを指定することはできません。", nameof(path));

            path.From.Wire(path);
            path.To.Wire(path);
            PathsKey.Add(path);
            return path;
        }

        public void PutStructures(ID3D11Device device, IPhysicsHost physicsHost, IEnumerable<LocatedModelTemplate> structures)
        {
            List<KinematicLocatedModelTemplate> structuresToMerge = [];
            foreach (LocatedModelTemplate structure in structures)
            {
                if (structure is KinematicLocatedModelTemplate kinematic && kinematic.CanMerge)
                {
                    KinematicLocatedModelTemplate compiled = new(physicsHost, kinematic.Model, structure.Pose * Pose);
                    structuresToMerge.Add(compiled);
                }
                else
                {
                    LocatedModel model = structure.Build(pose => pose * Pose);
                    ModelsKey.Add(model);
                }
            }

            if (0 < structuresToMerge.Count)
            {
                MergedKinematicLocatedModel mergedModel = MergedKinematicLocatedModel.Create(physicsHost, structuresToMerge);
                mergedModel.Model.CreateColliderDebugModel(device);
                mergedModel.Model.ColliderDebugModel!.Color = DebugColors[DebugColorIndex];
                ModelsKey.Add(mergedModel);
            }
        }

        public Pose GetConnectionPose(NetworkPort port, Pose targetPortOffset)
        {
            Pose offsetInv = Pose.Inverse(targetPortOffset);
            Pose pose = offsetInv * Pose.CreateRotationY(-float.Pi) * port.Offset * Pose;
            return pose;
        }

        public T ConnectNew<T>(NetworkPort port, PortDefinition targetPort, Func<int, int, Pose, T> elementFactory) where T : NetworkElement
        {
            Pose pose = GetConnectionPose(port, targetPort.Offset);

            T element = elementFactory(PlateX, PlateZ, pose);
            port.ConnectTo(element.Ports[targetPort.Name]);

            return element;
        }
    }
}
