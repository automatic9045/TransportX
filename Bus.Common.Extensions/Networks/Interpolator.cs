using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery.Networks;

namespace Bus.Common.Extensions.Networks
{
    public class Interpolator : NetworkEdge
    {
        private NetworkPort OutletKey;

        public override NetworkPort.Inlet Inlet { get; }
        public override NetworkPort Outlet => OutletKey;

        public Interpolator(int plateX, int plateZ, Matrix4x4 transform, LaneLayout connectionLayout) : base(plateX, plateZ, transform, false)
        {
            Inlet = new NetworkPort.Inlet(this, connectionLayout.CreateOpposition());
            OutletKey = new NetworkPort(this, Matrix4x4.Identity, Inlet.Layout.CreateOpposition());
        }

        public override void SetChild(NetworkElement child)
        {
            base.SetChild(child);

            Matrix4x4.Invert(Transform, out Matrix4x4 transformInverse);
            Matrix4x4 betweenPlates = GetPlateOffset(child).Transform;
            
            Matrix4x4 offset = transformInverse * betweenPlates * child.Transform;
            OutletKey = new NetworkPort(this, offset, Inlet.Layout.CreateOpposition());
        }
    }
}
