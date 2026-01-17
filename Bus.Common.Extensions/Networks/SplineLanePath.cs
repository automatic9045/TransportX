using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery.Networks;

namespace Bus.Common.Extensions.Networks
{
    public class SplineLanePath : LanePath
    {
        private readonly Spline ParentSpline;

        public override float Length => ParentSpline.Length;

        public SplineLanePath(LanePin from, LanePin to) : base(from, to)
        {
            ParentSpline = from.Port.Owner as Spline ?? throw new ArgumentException($"親が {nameof(Spline)} の進路端子である必要があります。", nameof(from));
        }

        public override Pose GetLocalPose(float at)
        {
            return Pose.CreateRotationY(float.Pi) * From.LocalPose * ParentSpline.GetPose(at / Length * ParentSpline.Length);
        }
    }
}
