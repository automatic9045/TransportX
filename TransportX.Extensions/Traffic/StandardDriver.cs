using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Traffic;

namespace TransportX.Extensions.Traffic
{
    public class StandardDriver : IDriver
    {
        private readonly float MaxSpeed = (Random.Shared.NextSingle() * 30 + 40) / 3.6f; // 40～70

        private readonly ILaneTracker LaneTracker;
        private readonly ITrafficSensor Sensor;

        private readonly float Acceleration;
        private readonly float MinDeceleration;
        private readonly float MaxDeceleration;

        private readonly float StopMargin;
        private readonly float TimeHeadway;

        public float CurrentAcceleration { get; private set; } = 0;
        float IDriver.Acceleration => CurrentAcceleration;

        public StandardDriver(ILaneTracker laneTracker, ITrafficSensor sensor,
            float acceleration, float minDeceleration, float maxDeceleration, float stopMargin, float timeHeadway)
        {
            LaneTracker = laneTracker;
            Sensor = sensor;

            Acceleration = acceleration;
            MinDeceleration = minDeceleration;
            MaxDeceleration = maxDeceleration;

            StopMargin = stopMargin;
            TimeHeadway = timeHeadway;
        }

        public virtual void Tick(TimeSpan elapsed)
        {
            if (!LaneTracker.IsEnabled || LaneTracker.Path is null) throw new InvalidOperationException();

            int direction = (int)LaneTracker.Heading;
            float speed = LaneTracker.SVelocity * direction;

            float cruiseAcceleration = speed < MaxSpeed ? Acceleration
                : MaxSpeed < speed ? -MinDeceleration
                : 0;

            float brakeAcceleration = float.MaxValue;
            if (Sensor.Target is not null)
            {
                float nextOffset = Sensor.IsTargetOncoming ? 0 : Sensor.Target.Length;
                float effectiveDistance = Sensor.DistanceToTarget - nextOffset - StopMargin;
                if (1 < float.Abs(speed)) effectiveDistance -= float.Max(0, speed) * TimeHeadway;
                if (effectiveDistance <= 0)
                {
                    brakeAcceleration = -MaxDeceleration;
                }
                else
                {
                    float nextSpeed = (Sensor.IsTargetOncoming ? -1 : 1) * Sensor.Target.SVelocity * (int)Sensor.Target.Heading;
                    if (nextSpeed < speed)
                    {
                        float deltaSpeed = speed - nextSpeed;
                        float requiredDeceleration = deltaSpeed * deltaSpeed / 2 / effectiveDistance;
                        if (MinDeceleration < requiredDeceleration)
                        {
                            brakeAcceleration = -float.Min(requiredDeceleration, MaxDeceleration);
                        }
                    }
                }
            }

            float acceleration = float.Min(cruiseAcceleration, brakeAcceleration);
            if (float.Abs(speed) < 1e-3f && acceleration < 0)
            {
                acceleration = 0; 
            }

            CurrentAcceleration = acceleration * direction;
        }
    }
}
