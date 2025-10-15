using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Rendering;
using Bus.Common.Scenery;
using Bus.Common.Scenery.Networks;

namespace Bus.Common.Extensions.Networks
{
    public class Interpolator : NetworkEdge
    {
        private ElementPath PathKey;

        public override LaneConnector Port { get; }
        public override ElementPath Path => PathKey;

        public Interpolator(int plateX, int plateZ, Matrix4x4 locator, LaneConnector pairedPort) : base(plateX, plateZ, locator, false)
        {
            Port = pairedPort.CreateOpposition();
            PathKey = new ElementPath(Matrix4x4.Identity, Port.CreateOpposition());
        }

        public override void SetChild(NetworkElement child)
        {
            base.SetChild(child);

            Matrix4x4.Invert(Locator, out Matrix4x4 locatorInverse);
            Matrix4x4 betweenPlates = Matrix4x4.CreateTranslation(Plate.Size * (child.PlateX - PlateX), 0, Plate.Size * (child.PlateZ - PlateZ));
            
            Matrix4x4 transition = locatorInverse * betweenPlates * child.Locator;
            PathKey = new ElementPath(transition, Port.CreateOpposition());
        }
    }
}
