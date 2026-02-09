using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery.Networks;
using Bus.Common.Traffic;
using Bus.Common.Rendering;

namespace Bus.Common.Extensions.Traffic
{
    public class HybridTrafficSensor : ITrafficSensor
    {
        protected readonly NetworkTrafficSensor NetworkSensor;
        protected readonly SpatialTrafficSensor SpatialSensor;

        protected ITrafficSensor CurrentSensor;

        public ITrafficParticipant? Target { get; private set; } = null;
        public bool IsTargetOncoming { get; private set; } = false;
        public float DistanceToTarget { get; private set; } = 0;

        public Vector4 NetworkDebugColor
        {
            get => NetworkSensor.DebugColor;
            set => NetworkSensor.DebugColor = value;
        }
        public Vector4 SpatialDebugColor
        {
            get => SpatialSensor.DebugColor;
            set => SpatialSensor.DebugColor = value;
        }
        Vector4 IDebugDrawable.DebugColor
        {
            get => NetworkDebugColor;
            set => NetworkDebugColor = value;
        }

        public string? DebugName
        {
            get => field;
            set
            {
                field = value;
                if (value is null)
                {
                    NetworkSensor.DebugName = SpatialSensor.DebugName = null;
                }
                else
                {
                    NetworkSensor.DebugName = $"{value}_Network";
                    SpatialSensor.DebugName = $"{value}_Spatial";
                }
            }
        }

        public HybridTrafficSensor(ILaneTracker laneTracker, ILocatable location, Func<ITrafficParticipant, bool> obstacleSkipCondition)
        {
            NetworkSensor = new NetworkTrafficSensor(laneTracker, location);
            SpatialSensor = new SpatialTrafficSensor(laneTracker, location,
                obstacle => obstacle != NetworkSensor.Target && obstacleSkipCondition(obstacle));

            CurrentSensor = NetworkSensor;
        }

        public void Dispose()
        {
            NetworkSensor.Dispose();
            SpatialSensor.Dispose();
        }

        public void Tick(IEnumerable<LanePathView> plannedRoute, IEnumerable<ITrafficParticipant> obstacles, TimeSpan elapsed)
        {
            NetworkSensor.Tick(plannedRoute, obstacles, elapsed);

            SpatialSensor.MaxDistance = NetworkSensor.DistanceToTarget;
            SpatialSensor.Tick(plannedRoute, obstacles, elapsed);

            CurrentSensor = NetworkSensor.DistanceToTarget <= SpatialSensor.DistanceToTarget ? NetworkSensor : SpatialSensor;

            Target = CurrentSensor.Target;
            IsTargetOncoming = CurrentSensor.IsTargetOncoming;
            DistanceToTarget = CurrentSensor.DistanceToTarget;
        }

        public void Draw(LocatedDrawContext context)
        {
            CurrentSensor.Draw(context);
        }
    }
}
