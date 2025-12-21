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

        public float ConvertS(float s)
        {
            return Reverse ? Source.Length - s : s;
        }

        public float InvertS(float convertedS)
        {
            return Reverse ? Source.Length - convertedS : convertedS;
        }

        public Matrix4x4 GetTransform(float convertedS) => DirectionMatrix * Source.GetTransform(InvertS(convertedS));
        public Matrix4x4 GetWorldTransform(float convertedS) => DirectionMatrix * Source.GetWorldTransform(InvertS(convertedS));
    }
}
