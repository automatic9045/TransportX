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
        protected new readonly Spline Parent;

        public override float Length { get; }

        public SplineLanePath(LanePin from, LanePin to) : base(from, to)
        {
            Parent = from.Parent as Spline ?? throw new ArgumentException($"親が {nameof(Spline)} の進路端子である必要があります。", nameof(from));
            Length = Parent.Length * (1 - from.Definition.Position.X * Parent.Curvature);
        }

        public override Matrix4x4 GetTransform(float at)
        {
            Matrix4x4 offset = Matrix4x4.CreateTranslation(new Vector3(From.Definition.Position, 0));
            return offset * Parent.GetTransform(at / Length * Parent.Length);
        }
    }
}
