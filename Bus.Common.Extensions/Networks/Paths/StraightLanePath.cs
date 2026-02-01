using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery.Networks;

using Bus.Common.Extensions.Mathematics;

namespace Bus.Common.Extensions.Networks.Paths
{
    public class StraightLanePath : LanePath
    {
        protected readonly LinearPoseCurve Curve;

        public override float Length => Curve.Length;

        public StraightLanePath(LanePin from, LanePin to) : base(from, to)
        {
            Curve = new LinearPoseCurve(Pose.CreateRotationY(float.Pi) * from.LocalPose, to.LocalPose);
        }

        public override Pose GetLocalPose(float at) => Curve.GetPose(at);
    }
}
