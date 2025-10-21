using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D11;

namespace Bus.Common.Scenery.Networks
{
    public abstract class NetworkNode : NetworkElement
    {
        public NetworkNode(int plateX, int plateZ, Matrix4x4 transform, bool isRoot) : base(plateX, plateZ, transform, isRoot)
        {
        }
    }
}
