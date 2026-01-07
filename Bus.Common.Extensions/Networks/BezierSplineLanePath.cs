using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery.Networks;

namespace Bus.Common.Extensions.Networks
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

        public override Matrix4x4 GetTransform(float at)
        {
            return Matrix4x4.CreateRotationY(float.Pi) * From.LocalTransform * ParentSpline.GetTransform(at / Length * ParentSpline.Length);
        }
    }
}
