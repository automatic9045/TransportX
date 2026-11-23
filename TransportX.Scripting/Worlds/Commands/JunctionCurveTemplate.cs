using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Mathematics;

using TransportX.Extensions.Mathematics;
using TransportX.Extensions.Network.Elements;

namespace TransportX.Scripting.Worlds.Commands
{
    public class JunctionCurveTemplate : CurveTemplateBase<JunctionCurveTemplate>
    {
        internal static JunctionCurveTemplate Empty(JunctionTemplate parent) => new(parent, string.Empty, Pose.Identity);

        public JunctionCurveTemplate(JunctionTemplate parent, string key, Pose startPose)
            : base(parent, key, startPose)
        {
        }

        internal JunctionCurveFactoryCommand Build()
        {
            IsFinalized = true;

            IPoseCurve curve = Curves.Count == 0 ? IPoseCurve.Empty : new CompositePoseCurve(Curves);
            return new JunctionCurveFactoryCommand(Parent.World, Key, curve, Props, Extrusions);
        }
    }
}
