using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;
using TransportX.Traffic;

namespace TransportX.Domains.RoadTraffic.Network
{
    public class PathDeflectionComponent : IComponent
    {
        public float Forward { get; }
        public float Backward { get; }

        public PathDeflectionComponent(float forward, float backward)
        {
            if (forward < -1 || 1 < forward) throw new ArgumentOutOfRangeException(nameof(forward));
            if (backward < -1 || 1 < backward) throw new ArgumentOutOfRangeException(nameof(backward));

            Forward = forward;
            Backward = backward;
        }

        public float GetDeflection(ParticipantDirection direction)
        {
            return direction switch
            {
                ParticipantDirection.Forward => Forward,
                ParticipantDirection.Backward => Backward,
                _ => throw new NotSupportedException(),
            };
        }
    }
}
