using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery.Networks;
using Bus.Common.Traffic;

namespace Bus.Common.Extensions.Traffic
{
    public class HybridTrafficSensor : ITrafficSensor
    {
        protected readonly NetworkTrafficSensor NetworkSensor;
        protected readonly SpatialTrafficSensor SpatialSensor;

        public ITrafficParticipant? Target { get; private set; } = null;
        public bool IsTargetOncoming { get; private set; } = false;
        public float DistanceToTarget { get; private set; } = 0;

        public HybridTrafficSensor(ILaneTracker laneTracker, IPoseSolver poseSolver, Func<ITrafficParticipant, bool> obstacleSkipCondition)
        {
            NetworkSensor = new NetworkTrafficSensor(laneTracker);
            SpatialSensor = new SpatialTrafficSensor(laneTracker, poseSolver,
                obstacle => obstacle != NetworkSensor.Target && obstacleSkipCondition(obstacle));
        }

        public void Tick(IEnumerable<LanePathView> plannedRoute, IEnumerable<ITrafficParticipant> obstacles, TimeSpan elapsed)
        {
            NetworkSensor.Tick(plannedRoute, obstacles, elapsed);

            SpatialSensor.MaxDistance = NetworkSensor.DistanceToTarget;
            SpatialSensor.Tick(plannedRoute, obstacles, elapsed);

            if (NetworkSensor.DistanceToTarget <= SpatialSensor.DistanceToTarget)
            {
                Target = NetworkSensor.Target;
                IsTargetOncoming = NetworkSensor.IsTargetOncoming;
                DistanceToTarget = NetworkSensor.DistanceToTarget;
            }
            else
            {
                Target = SpatialSensor.Target;
                IsTargetOncoming = SpatialSensor.IsTargetOncoming;
                DistanceToTarget = SpatialSensor.DistanceToTarget;
            }
        }
    }
}
