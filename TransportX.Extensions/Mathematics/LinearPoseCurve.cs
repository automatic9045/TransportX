using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Mathematics;

namespace TransportX.Extensions.Mathematics
{
    public class LinearPoseCurve : IPoseCurve
    {
        private readonly Vector3 TransitionUp;
        private readonly Vector3 Direction;

        public Pose StartPose { get; }
        public Pose EndPose { get; }
        public float Length { get; }

        public LinearPoseCurve(Pose startPose, Pose endPose)
        {
            StartPose = startPose;
            EndPose = endPose;

            Pose transition = EndPose * Pose.Inverse(StartPose);
            TransitionUp = transition.Up;
            Length = transition.Position.Length();
            Direction = transition.Position / Length;
        }

        public Pose GetPose(float s)
        {
            if (Length < 1e-3f) return StartPose;
            if (s <= 0) return StartPose;
            if (Length <= s) return EndPose;

            Vector3 up = Vector3.Lerp(Vector3.UnitY, TransitionUp, float.Clamp(s / Length, 0, 1));
            Pose transition = Pose.CreateWorldLH(Direction * s, Direction, up);
            return transition * StartPose;
        }
    }
}
