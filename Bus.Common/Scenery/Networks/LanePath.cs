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
        public NetworkElement Owner => From.Port.Owner;
        public LaneTrafficGroup AllowedTraffic => From.Definition.AllowedTraffic;
        public FlowDirections Directions => From.Definition.Directions.GetOpposition();

        public LanePin From { get; }
        public LanePin To { get; }

        private readonly List<ITrafficParticipant> ParticipantsKey = [];
        public IReadOnlyList<ITrafficParticipant> Participants => ParticipantsKey;

        public abstract float Length { get; }

        protected LanePath(LanePin from, LanePin to)
        {
            if (from.Port.Owner != to.Port.Owner) throw new ArgumentException($"同一 {nameof(NetworkElement)} 内のピンを指定する必要があります。", nameof(to));

            From = from;
            To = to;
        }

        public abstract Pose GetLocalPose(float at);

        public Pose GetPose(float at)
        {
            return GetLocalPose(at) * Owner.Pose;
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
