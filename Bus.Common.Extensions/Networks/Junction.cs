using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery;
using Bus.Common.Scenery.Networks;

namespace Bus.Common.Extensions.Networks
{
    public class Junction : NetworkNode
    {
        public override IReadOnlyList<NetworkPort> Ports { get; }

        private readonly List<LanePath> PathsKey = [];
        public override IReadOnlyList<LanePath> Paths => PathsKey;

        private readonly List<LocatedModel> ModelsKey = [];
        public override IReadOnlyList<LocatedModel> Models => ModelsKey;

        public Junction(int plateX, int plateZ, Matrix4x4 transform, IEnumerable<PortDefinition> ports)
            : base(plateX, plateZ, transform)
        {
            Ports = ports.Select(def => new NetworkPort(this, def.Offset, def.Layout)).ToArray();
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
    }
}
