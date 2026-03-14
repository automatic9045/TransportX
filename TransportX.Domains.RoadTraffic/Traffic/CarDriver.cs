using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Traffic;

using TransportX.Domains.RoadTraffic.Network;

namespace TransportX.Domains.RoadTraffic.Traffic
{
    public class CarDriver : IDriver
    {
        private readonly ILaneTracker LaneTracker;
        private readonly ITrafficSensor Sensor;

        private readonly float DriverAcceleration;
        private readonly float DriverDeceleration;

        public CarSpec Spec { get; }
        public DriverPersonality Personality { get; }

        public float Acceleration { get; private set; } = 0;

        public CarDriver(ILaneTracker laneTracker, ITrafficSensor sensor, CarSpec spec, DriverPersonality personality)
        {
            LaneTracker = laneTracker;
            Sensor = sensor;

            Spec = spec;
            Personality = personality;

            DriverAcceleration = float.Lerp(Spec.MinAcceleration, Spec.MaxAcceleration, Personality.Factor);
            DriverDeceleration = float.Lerp(Spec.MinDeceleration, Spec.MaxDeceleration, Personality.Factor * 0.5f);
        }

        public void Tick(TimeSpan elapsed)
        {
            if (!LaneTracker.IsEnabled || LaneTracker.Path is null) throw new InvalidOperationException();

            int direction = (int)LaneTracker.Heading;
            float speed = LaneTracker.SVelocity * direction;

            float maxSpeed = Spec.MaxSpeed;
            if (LaneTracker.Path.Components.TryGet<SpeedLimitComponent>(out SpeedLimitComponent? speedLimit))
            {
                maxSpeed = float.Min(maxSpeed, speedLimit.MaxSpeed);
            }

            float cruiseAcceleration = speed < maxSpeed ? DriverAcceleration
                : maxSpeed < speed ? -DriverDeceleration
                : 0;

            float brakeAcceleration = float.MaxValue;
            if (Sensor.Target is not null)
            {
                float stopMargin = float.IsNaN(Sensor.StopMargin) ? Personality.DefaultStopMargin : Sensor.StopMargin;
                float effectiveDistance = Sensor.DistanceToTarget - stopMargin;
                if (1 < float.Abs(speed)) effectiveDistance -= float.Max(0, speed) * Personality.TimeHeadway;
                if (effectiveDistance <= 0)
                {
                    brakeAcceleration = -Spec.MaxDeceleration;
                }
                else
                {
                    float nextSpeed = (Sensor.IsTargetOncoming ? -1 : 1) * Sensor.Target.SVelocity * (int)Sensor.Target.Heading;
                    if (nextSpeed < speed)
                    {
                        float deltaSpeed = speed - nextSpeed;
                        float requiredDeceleration = deltaSpeed * deltaSpeed / 2 / effectiveDistance;
                        if (DriverDeceleration < requiredDeceleration)
                        {
                            brakeAcceleration = -float.Min(requiredDeceleration, Spec.MaxDeceleration);
                        }
                    }
                }
            }

            float acceleration = float.Min(cruiseAcceleration, brakeAcceleration);
            if (float.Abs(speed) < 1e-3f && acceleration < 0)
            {
                acceleration = 0;
            }

            Acceleration = acceleration * direction;
        }
    }
}
