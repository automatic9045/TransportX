using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Spatial;

namespace TransportX.Network
{
    public abstract class NetworkNode : NetworkElement
    {
        public NetworkNode(WorldPose worldPose) : base(worldPose)
        {
        }
    }
}
