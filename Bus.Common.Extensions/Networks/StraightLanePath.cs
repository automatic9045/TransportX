using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery.Networks;

namespace Bus.Common.Extensions.Networks
{
    public class StraightLanePath : LanePath
    {
        protected readonly Matrix4x4 FromTransform;
        protected readonly Vector3 ToUp;
        protected readonly Vector3 Direction;

        public override float Length { get; }

        public StraightLanePath(LanePin from, LanePin to) : base(from, to)
        {
            FromTransform = Matrix4x4.CreateRotationY(float.Pi) * from.LocalTransform;
            Matrix4x4.Invert(FromTransform, out Matrix4x4 fromTransformInv);
            Matrix4x4 transition = to.LocalTransform * fromTransformInv;
            ToUp = Vector3.Normalize(Vector3.TransformNormal(Vector3.UnitY, transition));
            Length = transition.Translation.Length();
            Direction = transition.Translation / Length;
        }

        public override Matrix4x4 GetTransform(float at)
        {
            if (Length < 1e-3f) return FromTransform;

            Vector3 up = Vector3.Lerp(Vector3.UnitY, ToUp, float.Clamp(at / Length, 0, 1));
            Matrix4x4 transition = Matrix4x4.CreateWorld(Direction * at, -Direction, up);
            return transition * FromTransform;
        }
    }
}
