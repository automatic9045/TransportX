using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Mathematics;

using TransportX.Extensions.Network.Elements;

namespace TransportX.Scripting.Worlds.Commands
{
    public class JunctionCurveFactoryCommand : CurveFactoryCommandBase
    {
        internal static JunctionCurveFactoryCommand Empty(ScriptWorld world)
            => new(world, string.Empty, IPoseCurve.Empty, [], []);


        public JunctionCurveFactoryCommand(ScriptWorld world, string key, IPoseCurve curve, IEnumerable<SplineProp> props, IEnumerable<SplineExtrusion> extrusions)
            : base(world, key, curve, props, extrusions)
        {
        }
    }
}
