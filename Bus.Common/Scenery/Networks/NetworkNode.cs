using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Scenery.Networks
{
    public abstract class NetworkNode : NetworkElement
    {
        public NetworkNode(int plateX, int plateZ, Matrix4x4 transform) : base(plateX, plateZ, transform)
        {
        }
    }
}
