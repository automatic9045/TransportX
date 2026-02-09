using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

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

        public IDebugModel? DebugModel => throw new NotSupportedException();

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

        public void CreateDebugModel(ID3D11Device device)
        {
            NetworkSensor.CreateDebugModel(device);
            SpatialSensor.CreateDebugModel(device);

            NetworkSensor.DebugModel!.Color = new Vector4(0, 1, 1, 1);
            SpatialSensor.DebugModel!.Color = new Vector4(1, 0, 1, 1);
        }

        public void DrawDebug(LocatedDrawContext context)
        {
            CurrentSensor.DrawDebug(context);
        }
    }
}
