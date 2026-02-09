using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Network;

namespace TransportX.Extensions.Network.Elements
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

        public Pose GetPinLocalPose(int pinIndex)
        {
            return new Pose(new Vector3(Layout.Lanes[pinIndex].Position, 0)) * Offset;
        }
    }
}
