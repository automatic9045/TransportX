using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Collections;
using Bus.Common.Scenery;
using Bus.Common.Scenery.Networks;

namespace Bus.Common.Extensions.Networks
{
    public class Junction : NetworkNode
    {
        public override IReadOnlyKeyedList<string, NetworkPort> Ports { get; }

        private readonly List<LanePath> PathsKey = [];
        public override IReadOnlyList<LanePath> Paths => PathsKey;

        private readonly List<LocatedModel> ModelsKey = [];
        public override IReadOnlyList<LocatedModel> Models => ModelsKey;

        public Junction(int plateX, int plateZ, Pose pose, IEnumerable<PortDefinition> ports) : base(plateX, plateZ, pose)
        {
            Ports = new KeyedList<string, NetworkPort>(item => item.Name, ports.Select(def => new NetworkPort(def.Name, this, def.Offset, def.Layout)));
        }

        public LanePath Wire(LanePin from, LanePin to)
        {
            if (!Ports.Contains(from.Port)) throw new ArgumentException($"他の {nameof(NetworkElement)} に属しているピンを指定することはできません。", nameof(from));
            if (!Ports.Contains(to.Port)) throw new ArgumentException($"他の {nameof(NetworkElement)} に属しているピンを指定することはできません。", nameof(to));

            BezierLanePath path = new(from, to);
            from.Wire(path);
            to.Wire(path);
            PathsKey.Add(path);
            return path;
        }

        public void AddStructure(LocatedModel model)
        {
            ModelsKey.Add(model);
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
