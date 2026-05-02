using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;
using TransportX.Rendering;
using TransportX.Spatial;
using TransportX.Traffic;

namespace TransportX.Network
{
    public interface ILanePath : IDebugDrawable
    {
        string Name { get; }

        NetworkElement Owner { get; }
        LaneTrafficGroup AllowedTraffic { get; }
        FlowDirections Directions { get; }

        LanePin From { get; }
        LanePin To { get; }
        float Length { get; }

        IComponentCollection<IComponent> Components { get; }

        IReadOnlyList<ITrafficEntity> Entities { get; }

        string? DebugName { get; set; }

        Pose GetLocalPose(float at);
        WorldPose GetWorldPose(float at);
        LaneWidth GetWidth(float at);

        void Enter(ITrafficEntity entity);
        void Exit(ITrafficEntity entity);
    }
}
