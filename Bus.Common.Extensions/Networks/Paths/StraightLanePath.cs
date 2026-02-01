using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery.Networks;

namespace Bus.Common.Extensions.Networks.Paths
{
    public class StraightLanePath : LanePath
    {
        protected readonly Pose FromPose;
        protected readonly Vector3 ToUp;
        protected readonly Vector3 Direction;

        public override float Length { get; }

        public StraightLanePath(LanePin from, LanePin to) : base(from, to)
        {
            FromPose = Pose.CreateRotationY(float.Pi) * from.LocalPose;
            Pose fromPoseInv = Pose.Inverse(FromPose);
            Pose transition = to.LocalPose * fromPoseInv;
            ToUp = Vector3.Normalize(Pose.TransformNormal(Vector3.UnitY, transition));
            Length = transition.Position.Length();
            Direction = transition.Position / Length;
        }

        public override Pose GetLocalPose(float at)
        {
            if (Length < 1e-3f) return FromPose;

            Vector3 up = Vector3.Lerp(Vector3.UnitY, ToUp, float.Clamp(at / Length, 0, 1));
            Pose transition = Pose.CreateWorldLH(Direction * at, Direction, up);
            return transition * FromPose;
        }
    }
}
