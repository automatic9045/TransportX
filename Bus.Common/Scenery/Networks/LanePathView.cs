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
        public LanePath Source { get; }
        public bool Reverse { get; }

        public LanePin From => Reverse ? Source.To : Source.From;
        public LanePin To => Reverse ? Source.From : Source.To;

        private Matrix4x4 DirectionMatrix { get; }

        public LanePathView(LanePath source, bool reverse)
        {
            Source = source;
            Reverse = reverse;

            DirectionMatrix = Reverse ? Matrix4x4.CreateRotationY(float.Pi) : Matrix4x4.Identity;
        }

        public LanePathView(LanePath source, ParticipantDirection heading) : this(source, heading == ParticipantDirection.Backward)
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

        public Matrix4x4 GetTransform(float viewS) => DirectionMatrix * Source.GetTransform(FromViewS(viewS));
        public Matrix4x4 GetWorldTransform(float viewS) => DirectionMatrix * Source.GetWorldTransform(FromViewS(viewS));
    }
}
