using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Rendering;
using Bus.Common.Traffic;

namespace Bus.Common.Scenery.Networks
{
    public interface ILanePath : IDebugVisualizable
    {
        NetworkElement Owner { get; }
        LaneTrafficGroup AllowedTraffic { get; }
        FlowDirections Directions { get; }

        LanePin From { get; }
        LanePin To { get; }
        float Length { get; }

        IReadOnlyList<ITrafficParticipant> Participants { get; }

        string? DebugName { get; set; }

        Pose GetLocalPose(float at);
        Pose GetPose(float at);

        void Enter(ITrafficParticipant participant);
        void Exit(ITrafficParticipant participant);
    }
}
