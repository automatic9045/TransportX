using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Mathematics;

namespace TransportX.Extensions.Mathematics
{
    public class BezierPoseCurve : IPoseCurve
    {
        private readonly CubicBezier3D Curve;
        private readonly Vector3 TransitionUp;

        public Pose StartPose { get; }
        public Pose EndPose { get; }
        public float Length => Curve.Length;

        public BezierPoseCurve(Pose startPose, Pose endPose, float handleScale = 0.5f)
        {
            StartPose = startPose;
            EndPose = endPose;

            Pose transition = EndPose * Pose.Inverse(StartPose);

            Vector3 p0 = Vector3.Zero;
            Vector3 p3 = transition.Position;

            float handleLength = p3.Length() * handleScale;

            Vector3 p1 = p0 + Vector3.UnitZ * handleLength;

            Vector3 toForward = Vector3.Normalize(Pose.TransformNormal(Vector3.UnitZ, transition));
            Vector3 p2 = p3 - toForward * handleLength;

            Curve = new CubicBezier3D(p0, p1, p2, p3);

            TransitionUp = transition.Up;
        }

        public Pose GetPose(float s)
        {
            float t = Curve.GetT(s);

            Vector3 position = Curve.GetPoint(t);
            Vector3 tangent = Curve.GetTangent(t);

            Vector3 up = Vector3.Normalize(Vector3.Lerp(Vector3.UnitY, TransitionUp, t));
            Pose localPose = Pose.CreateWorldLH(position, tangent, up);

            return localPose * StartPose;
        }
    }
}
