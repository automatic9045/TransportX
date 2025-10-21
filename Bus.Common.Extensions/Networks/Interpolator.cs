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
        private ElementPath PathKey;

        public override LaneConnector Port { get; }
        public override ElementPath Path => PathKey;

        public Interpolator(int plateX, int plateZ, Matrix4x4 transform, LaneConnector pairedPort) : base(plateX, plateZ, transform, false)
        {
            Port = pairedPort.CreateOpposition();
            PathKey = new ElementPath(Matrix4x4.Identity, Port.CreateOpposition());
        }

        public override void SetChild(NetworkElement child)
        {
            base.SetChild(child);

            Matrix4x4.Invert(Transform, out Matrix4x4 transformInverse);
            Matrix4x4 betweenPlates = GetPlateOffset(child).Transform;
            
            Matrix4x4 transition = transformInverse * betweenPlates * child.Transform;
            PathKey = new ElementPath(transition, Port.CreateOpposition());
        }
    }
}
