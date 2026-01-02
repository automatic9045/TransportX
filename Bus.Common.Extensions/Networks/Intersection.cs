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
    public class Intersection : NetworkNode
    {
        public override NetworkPort.Inlet Inlet { get; }
        public override IReadOnlyList<NetworkPort> Outlets { get; }

        public Intersection(int plateX, int plateZ, Matrix4x4 transform, LaneLayout inlet, IEnumerable<OutletDefinition> outlets)
            : base(plateX, plateZ, transform)
        {
            Inlet = new NetworkPort.Inlet(this, inlet);
            Outlets = outlets.Select(def => new NetworkPort(this, def.Offset, def.Layout)).ToArray();
        }

        public LanePath Wire(LanePin from, LanePin to)
        {
            if (!Ports.Contains(from.Port)) throw new ArgumentException($"他の {nameof(NetworkElement)} に属しているピンを指定することはできません。", nameof(from));
            if (!Ports.Contains(to.Port)) throw new ArgumentException($"他の {nameof(NetworkElement)} に属しているピンを指定することはできません。", nameof(to));

            StraightLanePath path = new(from, to);
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
