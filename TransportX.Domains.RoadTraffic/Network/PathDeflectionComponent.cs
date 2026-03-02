using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;
using TransportX.Network;
using TransportX.Traffic;

using TransportX.Scripting;

namespace TransportX.Domains.RoadTraffic.Network
{
    public class PathDeflectionComponent : ITemplateComponent<ILanePath>
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

        void ITemplateComponent<ILanePath>.Build(ILanePath path, IErrorCollector errorCollector)
        {
            path.Components.Add(this);
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

        public float GetDeflection(bool reverse)
        {
            return reverse ? Backward : Forward;
        }
    }
}
