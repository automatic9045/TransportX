using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Network;
using TransportX.Spatial;

using TransportX.Extensions.Mathematics;

namespace TransportX.Extensions.Network.Paths
{
    public class StraightLanePath : LanePath
    {
        protected readonly LinearPoseCurve Curve;

        public override float Length => Curve.Length;

        public StraightLanePath(string name, LanePin from, LanePin to) : base(name, from, to)
        {
            Curve = new LinearPoseCurve(Pose.CreateRotationY(float.Pi) * from.LocalPose, to.LocalPose);
        }

        protected override Pose GetLocalPoseCore(float at) => Curve.GetPose(at);

        public override LaneWidth GetWidth(float at)
        {
            return LaneWidth.Lerp(FromWidth, To.Definition.Width, at / Length);
        }

        public override float ProjectToS(in WorldPose pose)
        {
            if (Length < 1e-3f) return 0;

            WorldPose startPose = GetWorldPose(0);
            WorldPose endPose = GetWorldPose(Length);

            Vector3 line = startPose.GetOffset(endPose);
            float lineLengthSquared = line.LengthSquared();
            if (lineLengthSquared < 1e-6f) return 0;

            Vector3 targetOffset = startPose.GetOffset(pose);
            float projectionRatio = Vector3.Dot(targetOffset, line) / lineLengthSquared;

            return float.Clamp(projectionRatio * Length, 0, Length);
        }
    }
}
