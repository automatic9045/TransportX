using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Network
{
    public abstract class NetworkNode : NetworkElement
    {
        public NetworkNode(int plateX, int plateZ, Pose pose) : base(plateX, plateZ, pose)
        {
        }
    }
}
