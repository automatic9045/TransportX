using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Extensions.Mathematics
{
    public class LinearPoseCurve : PoseCurveBase
    {
        private readonly Vector3 TransitionUp;
        private readonly Vector3 Direction;

        public override float Length { get; }

        public LinearPoseCurve(Pose from, Pose to) : base(from, to)
        {
            Pose fromInv = Pose.Inverse(From);
            Pose transition = to * fromInv;
            TransitionUp = transition.Up;
            Length = transition.Position.Length();
            Direction = transition.Position / Length;
        }

        public override Pose GetPose(float at)
        {
            if (Length < 1e-3f) return From;
            if (at <= 0) return From;
            if (Length <= at) return To;

            Vector3 up = Vector3.Lerp(Vector3.UnitY, TransitionUp, float.Clamp(at / Length, 0, 1));
            Pose transition = Pose.CreateWorldLH(Direction * at, Direction, up);
            return transition * From;
        }
    }
}
