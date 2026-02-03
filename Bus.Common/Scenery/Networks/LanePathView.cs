using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Traffic;

namespace Bus.Common.Scenery.Networks
{
    public readonly struct LanePathView
    {
        public ILanePath Source { get; }
        public bool Reverse { get; }

        public LanePin From => Reverse ? Source.To : Source.From;
        public LanePin To => Reverse ? Source.From : Source.To;

        private Pose DirectionPose { get; }

        public LanePathView(ILanePath source, bool reverse)
        {
            Source = source;
            Reverse = reverse;

            DirectionPose = Reverse ? Pose.CreateRotationY(float.Pi) : Pose.Identity;
        }

        public LanePathView(ILanePath source, ParticipantDirection heading) : this(source, heading == ParticipantDirection.Backward)
        {
        }

        public float ToViewS(float s)
        {
            return Reverse ? Source.Length - s : s;
        }

        public float FromViewS(float viewS)
        {
            return Reverse ? Source.Length - viewS : viewS;
        }

        public float ToViewVelocity(float velocity)
        {
            return Reverse ? -velocity : velocity;
        }

        public float FromViewVelocity(float viewVelocity)
        {
            return Reverse ? -viewVelocity : viewVelocity;
        }

        public Pose GetLocalPose(float viewS) => DirectionPose * Source.GetLocalPose(FromViewS(viewS));
        public Pose GetPose(float viewS) => DirectionPose * Source.GetPose(FromViewS(viewS));
    }
}
