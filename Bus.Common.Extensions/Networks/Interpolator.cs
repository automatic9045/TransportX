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
        private NetworkPort PortKey;

        public override LaneLayout Layout { get; }
        public override NetworkPort Port => PortKey;

        public Interpolator(int plateX, int plateZ, Matrix4x4 transform, LaneLayout connectionLayout) : base(plateX, plateZ, transform, false)
        {
            Layout = connectionLayout.CreateOpposition();
            PortKey = new NetworkPort(Matrix4x4.Identity, Layout.CreateOpposition());
        }

        public override void SetChild(NetworkElement child)
        {
            base.SetChild(child);

            Matrix4x4.Invert(Transform, out Matrix4x4 transformInverse);
            Matrix4x4 betweenPlates = GetPlateOffset(child).Transform;
            
            Matrix4x4 transition = transformInverse * betweenPlates * child.Transform;
            PortKey = new NetworkPort(transition, Layout.CreateOpposition());
        }
    }
}
