using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Extensions.Mathematics
{
    public class BezierPoseCurve : PoseCurveBase
    {
        private readonly CubicBezier3D Curve;
        private readonly Vector3 TransitionUp;

        public override float Length => Curve.Length;

        public BezierPoseCurve(Pose from, Pose to, float? controlScale = null) : base(from, to)
        {
            Pose fromInv = Pose.Inverse(From);
            Pose transition = to * fromInv;

            Vector3 p0 = Vector3.Zero;
            Vector3 p3 = transition.Position;

            float dist = p3.Length();
            float handleLength = controlScale ?? dist * 0.5f;

            Vector3 p1 = p0 + Vector3.UnitZ * handleLength;

            Vector3 toForward = Vector3.Normalize(Pose.TransformNormal(Vector3.UnitZ, transition));
            Vector3 p2 = p3 - toForward * handleLength;

            Curve = new CubicBezier3D(p0, p1, p2, p3);

            TransitionUp = transition.Up;
        }

        public override Pose GetPose(float at)
        {
            float t = Curve.GetT(at);

            Vector3 position = Curve.GetPoint(t);
            Vector3 tangent = Curve.GetTangent(t);

            Vector3 up = Vector3.Normalize(Vector3.Lerp(Vector3.UnitY, TransitionUp, t));
            Pose localPose = Pose.CreateWorldLH(position, tangent, up);

            return localPose * From;
        }
    }
}
