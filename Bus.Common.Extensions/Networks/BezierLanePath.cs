using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery.Networks;

namespace Bus.Common.Extensions.Networks
{
    public class BezierLanePath : LanePath
    {
        private readonly CubicBezier3D Curve;

        private readonly Matrix4x4 FromTransform;
        private readonly Vector3 FromUp;
        private readonly Vector3 ToUp;

        public override float Length => Curve.Length;

        public BezierLanePath(LanePin from, LanePin to, float? controlScale = null) : base(from, to)
        {
            FromTransform = Matrix4x4.CreateRotationY(float.Pi) * from.LocalTransform;
            Matrix4x4.Invert(FromTransform, out Matrix4x4 fromTransformInv);
            Matrix4x4 toInFromSpace = to.LocalTransform * fromTransformInv;

            Vector3 p0 = Vector3.Zero;
            Vector3 p3 = toInFromSpace.Translation;

            float dist = p3.Length();
            float handleLength = controlScale ?? dist * 0.5f;

            Vector3 p1 = p0 + Vector3.UnitZ * handleLength;

            Vector3 toForward = Vector3.Normalize(Vector3.TransformNormal(Vector3.UnitZ, toInFromSpace));
            Vector3 p2 = p3 - toForward * handleLength;

            Curve = new CubicBezier3D(p0, p1, p2, p3);

            FromUp = Vector3.UnitY;
            ToUp = Vector3.Normalize(Vector3.TransformNormal(Vector3.UnitY, toInFromSpace));
        }

        public override Matrix4x4 GetTransform(float at)
        {
            float t = Curve.GetT(at);

            Vector3 position = Curve.GetPoint(t);
            Vector3 tangent = Curve.GetTangent(t);

            Vector3 up = Vector3.Normalize(Vector3.Lerp(FromUp, ToUp, t));
            Matrix4x4 localTransform = Matrix4x4.CreateWorld(position, -tangent, up);

            return localTransform * FromTransform;
        }
    }
}
