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
        public string Name { get; }
        public LaneLayout Layout { get; }
        public Pose Offset { get; }

        public PortDefinition(string name, LaneLayout layout, Pose offset)
        {
            Name = name;
            Layout = layout;
            Offset = offset;
        }
    }
}
