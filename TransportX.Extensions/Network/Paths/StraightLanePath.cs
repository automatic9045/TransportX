using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Network;

using TransportX.Extensions.Mathematics;

namespace TransportX.Extensions.Network.Paths
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

        public override LaneWidth GetWidth(float at)
        {
            return LaneWidth.Lerp(FromWidth, To.Definition.Width, at / Length);
        }
    }
}
