using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Traffic;

namespace Bus.Common.Scenery.Networks
{
    public abstract class LanePath
    {
        public NetworkElement Parent => From.Parent;
        public LaneKind Kind => From.Definition.Kind;
        public FlowDirections Directions => From.Definition.Directions;

        public LanePin From { get; }
        public LanePin To { get; }

        private readonly List<ITrafficParticipant> ParticipantsKey = [];
        public IReadOnlyList<ITrafficParticipant> Participants => ParticipantsKey;

        public abstract float Length { get; }

        protected LanePath(LanePin from, LanePin to)
        {
            From = from;
            To = to;
        }

        public abstract Matrix4x4 GetTransform(float at);

        public Matrix4x4 GetWorldTransform(float at)
        {
            return GetTransform(at) * Parent.Transform;
        }

        public virtual void Enter(ITrafficParticipant participant)
        {
            ParticipantsKey.Add(participant);
        }

        public virtual void Exit(ITrafficParticipant participant)
        {
            ParticipantsKey.Remove(participant);
        }
    }
}
