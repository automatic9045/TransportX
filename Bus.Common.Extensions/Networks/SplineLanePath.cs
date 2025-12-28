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
        protected readonly Spline ParentSpline;

        public override float Length { get; }

        public SplineLanePath(LanePin from, LanePin to) : base(from, to)
        {
            ParentSpline = from.Port.Owner as Spline ?? throw new ArgumentException($"親が {nameof(Spline)} の進路端子である必要があります。", nameof(from));
            Length = ParentSpline.Length * (1 - from.Definition.Position.X * ParentSpline.Curvature);
        }

        public override Matrix4x4 GetTransform(float at)
        {
            return From.LocalTransform * ParentSpline.GetTransform(at / Length * ParentSpline.Length);
        }
    }
}
