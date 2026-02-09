using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Network;

using Bus.Common.Extensions.Mathematics;

namespace Bus.Common.Extensions.Network.Paths
{
    public class BezierLanePath : LanePath
    {
        protected readonly BezierPoseCurve Curve;

        public override float Length => Curve.Length;

        public BezierLanePath(LanePin from, LanePin to) : base(from, to)
        {
            Curve = new BezierPoseCurve(Pose.CreateRotationY(float.Pi) * from.LocalPose, to.LocalPose);
        }

        public override Pose GetLocalPose(float at) => Curve.GetPose(at);

        public override LaneWidth GetWidth(float at)
        {
            return LaneWidth.Lerp(FromWidth, To.Definition.Width, at / Length);
        }
    }
}
