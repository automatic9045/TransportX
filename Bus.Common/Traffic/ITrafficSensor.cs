using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery.Networks;

namespace Bus.Common.Traffic
{
    public interface ITrafficSensor
    {
        ITrafficParticipant? Target { get; }
        bool IsTargetOncoming { get; }
        float DistanceToTarget { get; }

        void Tick(IEnumerable<LanePathView> plannedRoute, IEnumerable<ITrafficParticipant> obstacles, TimeSpan elapsed);
    }
}
