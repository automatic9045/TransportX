using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Network;
using TransportX.Rendering;

namespace TransportX.Traffic
{
    public interface ITrafficSensor : IDebugDrawable
    {
        float MaxDistance { get; set; }

        ITrafficParticipant? Target { get; }
        bool IsTargetOncoming { get; }
        float DistanceToTarget { get; }
        float StopMargin { get; }

        string? DebugName { get; set; }

        void Tick(IReadOnlyCollection<LanePathView> plannedRoute, IEnumerable<ITrafficParticipant> obstacles, TimeSpan elapsed);
    }
}
