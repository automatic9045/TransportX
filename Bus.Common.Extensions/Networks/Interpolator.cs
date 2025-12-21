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
        private NetworkOutlet OutletKey;

        public override LaneLayout Layout { get; }
        public override NetworkOutlet Outlet => OutletKey;

        public Interpolator(int plateX, int plateZ, Matrix4x4 transform, LaneLayout connectionLayout) : base(plateX, plateZ, transform, false)
        {
            Layout = connectionLayout.CreateOpposition();
            OutletKey = new NetworkOutlet(Matrix4x4.Identity, Layout.CreateOpposition());
        }

        public override void SetChild(NetworkElement child)
        {
            base.SetChild(child);

            Matrix4x4.Invert(Transform, out Matrix4x4 transformInverse);
            Matrix4x4 betweenPlates = GetPlateOffset(child).Transform;
            
            Matrix4x4 transition = transformInverse * betweenPlates * child.Transform;
            OutletKey = new NetworkOutlet(transition, Layout.CreateOpposition());
        }
    }
}
