using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Network;
using TransportX.Traffic;
using TransportX.Rendering;

namespace TransportX.Extensions.Traffic
{
    public class CompositeTrafficSensor : ITrafficSensor
    {
        protected readonly IReadOnlyList<ITrafficSensor> Sensors;

        protected ITrafficSensor CurrentSensor;

        public float MaxDistance { get; set; } = float.MaxValue;

        public ITrafficParticipant? Target => CurrentSensor.Target;
        public bool IsTargetOncoming => CurrentSensor.IsTargetOncoming;
        public float DistanceToTarget => CurrentSensor.DistanceToTarget;
        public float StopMargin => CurrentSensor.StopMargin;

        Vector4 IDebugDrawable.DebugColor
        {
            get => CurrentSensor.DebugColor;
            set => CurrentSensor.DebugColor = value;
        }

        public string? DebugName
        {
            get => field;
            set
            {
                field = value;
                if (value is null)
                {
                    for (int i = 0; i < Sensors.Count; i++) Sensors[i].DebugName = null;
                }
                else
                {
                    for (int i = 0; i < Sensors.Count; i++) Sensors[i].DebugName = $"{value}_{i}";
                }
            }
        }

        public CompositeTrafficSensor(IReadOnlyList<ITrafficSensor> sensors)
        {
            if (sensors.Count == 0) throw new ArgumentException("1 個以上のセンサーを指定する必要があります。", nameof(sensors));

            Sensors = sensors;
            CurrentSensor = Sensors[0];
        }

        public void Dispose()
        {
            for (int i = 0; i < Sensors.Count; i++) Sensors[i].Dispose();
        }

        public void Tick(IReadOnlyCollection<LanePathView> plannedRoute, IEnumerable<ITrafficParticipant> obstacles, TimeSpan elapsed)
        {
            ITrafficSensor? currentSensor = null;
            for (int i = 0; i < Sensors.Count; i++)
            {
                ITrafficSensor sensor = Sensors[i];
                float searchDistance = currentSensor is null ? MaxDistance : currentSensor.DistanceToTarget + currentSensor.StopMargin;
                sensor.MaxDistance = searchDistance;
                sensor.Tick(plannedRoute, obstacles, elapsed);
                if (currentSensor is null || sensor.DistanceToTarget + sensor.StopMargin < searchDistance) currentSensor = sensor;
            }

            CurrentSensor = currentSensor!;
        }

        public void Draw(in LocatedDrawContext context)
        {
            CurrentSensor.Draw(context);
        }
    }
}
