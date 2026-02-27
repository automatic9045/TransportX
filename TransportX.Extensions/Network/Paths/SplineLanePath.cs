using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Network;

using TransportX.Extensions.Network.Elements;

namespace TransportX.Extensions.Network.Paths
{
    public class SplineLanePath : LanePath
    {
        private readonly Spline ParentSpline;

        public override float Length => ParentSpline.Length;

        public SplineLanePath(LanePin from, LanePin to) : base(from, to)
        {
            ParentSpline = from.Port.Owner as Spline ?? throw new ArgumentException($"親が {nameof(Spline)} の進路端子である必要があります。", nameof(from));
        }

        protected override Pose GetLocalPoseCore(float at)
        {
            return Pose.CreateRotationY(float.Pi) * From.LocalPose * ParentSpline.GetPose(at / Length * ParentSpline.Length);
        }

        public override LaneWidth GetWidth(float at)
        {
            return LaneWidth.Lerp(FromWidth, To.Definition.Width, at / Length);
        }
    }
}
