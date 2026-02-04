using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery.Networks;

using Bus.Common.Extensions.Networks.Elements;

namespace Bus.Common.Extensions.Networks.Paths
{
    public class BezierSplineLanePath : LanePath
    {
        protected readonly BezierSpline ParentSpline;

        public override float Length { get; }

        public BezierSplineLanePath(LanePin from, LanePin to) : base(from, to)
        {
            ParentSpline = from.Port.Owner as BezierSpline ?? throw new ArgumentException($"親が {nameof(BezierSpline)} の進路端子である必要があります。", nameof(from));
            Length = ParentSpline.Length; // TODO: 正確に計算するようにする
        }

        public override Pose GetLocalPose(float at)
        {
            return Pose.CreateRotationY(float.Pi) * From.LocalPose * ParentSpline.GetPose(at / Length * ParentSpline.Length);
        }

        public override LaneWidth GetWidth(float at)
        {
            return LaneWidth.Lerp(FromWidth, To.Definition.Width, at / Length);
        }
    }
}
