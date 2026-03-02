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
    public class BezierSplineLanePath : LanePath
    {
        protected readonly BezierSpline ParentSpline;

        public override float Length { get; }

        public BezierSplineLanePath(string name, LanePin from, LanePin to) : base(name, from, to)
        {
            ParentSpline = from.Port.Owner as BezierSpline ?? throw new ArgumentException($"親が {nameof(BezierSpline)} の進路端子である必要があります。", nameof(from));
            Length = ParentSpline.Length; // TODO: 正確に計算するようにする
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
