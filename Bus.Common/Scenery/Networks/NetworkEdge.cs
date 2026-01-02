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
        public override IReadOnlyList<NetworkPort> Outlets => [Outlet];
        public abstract NetworkPort Outlet { get; }

        public NetworkEdge(int plateX, int plateZ, Matrix4x4 transform) : base(plateX, plateZ, transform)
        {
        }

        public virtual void SetChild(NetworkElement child)
        {
            Outlet.ConnectTo(child.Inlet);
        }
    }
}
