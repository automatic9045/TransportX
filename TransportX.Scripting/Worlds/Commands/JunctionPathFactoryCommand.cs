using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;
using TransportX.Diagnostics;
using TransportX.Network;

using TransportX.Extensions.Network.Elements;
using TransportX.Extensions.Network.Paths;

namespace TransportX.Scripting.Worlds.Commands
{
    public class JunctionPathFactoryCommand : CurveFactoryCommandBase
    {
        private static LanePin EmptyPin(NetworkElement owner) => new(new NetworkPort(string.Empty, owner, Pose.Identity, new LaneLayout()), owner.Ports.Count);
        internal static JunctionPathFactoryCommand Empty(ScriptWorld world, NetworkElement owner)
            => new(world, string.Empty, new StraightLanePath(string.Empty, EmptyPin(owner), EmptyPin(owner)), [], []);


        public ILanePath Path { get; }

        public IComponentCollection<ITemplateComponent<ILanePath>> Components { get; } = new ComponentCollection<ITemplateComponent<ILanePath>>();

        public JunctionPathFactoryCommand(ScriptWorld world, string key, ILanePath path, IEnumerable<SplineProp> props, IEnumerable<SplineExtrusion> extrusions)
            : base(world, key, path, props, extrusions)
        {
            Path = path;
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
