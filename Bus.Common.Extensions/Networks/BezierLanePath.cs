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

        private readonly Pose FromPose;
        private readonly Vector3 FromUp;
        private readonly Vector3 ToUp;

        public override float Length => Curve.Length;

        public BezierLanePath(LanePin from, LanePin to, float? controlScale = null) : base(from, to)
        {
            FromPose = Pose.CreateRotationY(float.Pi) * from.LocalPose;
            Pose fromPoseInv = FromPose.Inverse();
            Pose transition = to.LocalPose * fromPoseInv;

            Vector3 p0 = Vector3.Zero;
            Vector3 p3 = transition.Position;

            float dist = p3.Length();
            float handleLength = controlScale ?? dist * 0.5f;

            Vector3 p1 = p0 + Vector3.UnitZ * handleLength;

            Vector3 toForward = Vector3.Normalize(Pose.TransformNormal(Vector3.UnitZ, transition));
            Vector3 p2 = p3 - toForward * handleLength;

            Curve = new CubicBezier3D(p0, p1, p2, p3);

            FromUp = Vector3.UnitY;
            ToUp = Vector3.Normalize(Pose.TransformNormal(Vector3.UnitY, transition));
        }

        public override Pose GetLocalPose(float at)
        {
            float t = Curve.GetT(at);

            Vector3 position = Curve.GetPoint(t);
            Vector3 tangent = Curve.GetTangent(t);

            Vector3 up = Vector3.Normalize(Vector3.Lerp(FromUp, ToUp, t));
            Pose localPose = Pose.CreateWorldLH(position, tangent, up);

            return localPose * FromPose;
        }
    }
}
