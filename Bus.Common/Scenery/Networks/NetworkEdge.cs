using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Scenery.Networks
{
    public abstract class NetworkEdge : NetworkElement
    {
        public abstract NetworkPort Inlet { get; }
        public abstract NetworkPort Outlet { get; }
        public override IReadOnlyList<NetworkPort> Ports => [Inlet, Outlet];

        public NetworkEdge(int plateX, int plateZ, Matrix4x4 transform) : base(plateX, plateZ, transform)
        {
        }

        public virtual void SetChild(NetworkEdge child)
        {
            Outlet.ConnectTo(child.Inlet);
        }
    }
}
