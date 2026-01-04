using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery.Networks;

namespace Bus.Common.Extensions.Networks
{
    public class PortDefinition
    {
        public LaneLayout Layout { get; }
        public Matrix4x4 Offset { get; }

        public PortDefinition(LaneLayout layout, Matrix4x4 offset)
        {
            Layout = layout;
            Offset = offset;
        }
    }
}
