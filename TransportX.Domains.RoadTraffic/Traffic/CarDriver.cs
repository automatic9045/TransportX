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
        private readonly float DriverMinDeceleration;
        private readonly float ReaccelerateThreshold;

        private float OldRequestedBrakeAcceleration = 0;
        private float OldBrakeAcceleration = 0;

        public CarSpec Spec { get; }
        public DriverPersonality Personality { get; }

        public float Acceleration { get; private set; } = 0;

        public CarDriver(ILaneTracker laneTracker, ITrafficSensor sensor, CarSpec spec, DriverPersonality personality)
        {
            LaneTracker = laneTracker;
            Sensor = sensor;

            Spec = spec;
            Personality = personality;

            DriverAcceleration = float.Lerp(Spec.MinThrottle, Spec.MaxThrottle, Personality.Factor);
            DriverDeceleration = float.Lerp(Spec.MinBrake, Spec.MaxBrake, Personality.Factor);
            DriverMinDeceleration = Spec.MinBrake / 2;
            ReaccelerateThreshold = float.Lerp(1.5f, 0.5f, Personality.Factor);
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

            float cruiseAcceleration = speed < maxSpeed - ReaccelerateThreshold ? DriverAcceleration
                : maxSpeed < speed ? -DriverDeceleration
                : 0;

            float requestedBrakeAcceleration = float.MaxValue;
            bool isEmergencyBrake = false;
            if (Sensor.Target is not null)
            {
                float physicalDistance = Sensor.DistanceToTarget - Sensor.StopMargin;
                float effectiveDistance = physicalDistance -
                    speed * Personality.TimeHeadway * float.Clamp((speed - Personality.CreepSpeed) / Personality.CreepSpeed * 1, 0, 1);

                float nextSpeed = (Sensor.IsTargetOncoming ? -1 : 1) * Sensor.Target.SVelocity * (int)Sensor.Target.Heading;

                float startUpDistanceThreshold = DriverAcceleration * Personality.StartUpReactionTime * Personality.StartUpReactionTime / 2;
                float startUpSpeedThreshold = DriverAcceleration * Personality.StartUpReactionTime;

                if (nextSpeed < speed)
                {
                    float deltaSpeed = speed - nextSpeed;
                    float requiredAcceleration = -deltaSpeed * deltaSpeed / 2 / float.Max(1e-3f, effectiveDistance);
                    if (requiredAcceleration <= -Spec.MinBrake)
                    {
                        if (OldRequestedBrakeAcceleration <= -DriverMinDeceleration || requiredAcceleration <= -DriverDeceleration)
                        {
                            requestedBrakeAcceleration = float.Max(requiredAcceleration, -Spec.EmergencyBrake);
                            isEmergencyBrake = requestedBrakeAcceleration < -(DriverDeceleration + Spec.EmergencyBrake) / 2;
                        }
                        else
                        {
                            cruiseAcceleration = float.Min(cruiseAcceleration, 0);
                        }
                    }
                    else if (OldRequestedBrakeAcceleration < 0)
                    {
                        cruiseAcceleration = float.Min(cruiseAcceleration, 0);
                    }
                }

                if (requestedBrakeAcceleration == float.MaxValue)
                {
                    if (effectiveDistance <= 0
                        || (OldRequestedBrakeAcceleration <= -DriverMinDeceleration && effectiveDistance < DriverMinDeceleration * Personality.TimeHeadway)
                        || (physicalDistance < startUpDistanceThreshold && float.Abs(nextSpeed) < startUpSpeedThreshold))
                    {
                        requestedBrakeAcceleration = -DriverMinDeceleration;
                    }
                }
            }

            float brakeAcceleration;
            if (requestedBrakeAcceleration == float.MaxValue)
            {
                brakeAcceleration = float.MaxValue;
            }
            else
            {
                float brakeJerk = isEmergencyBrake ? 40 : 1;
                float baseBrakeAcceleration = OldBrakeAcceleration + brakeJerk * float.Sign(requestedBrakeAcceleration - OldBrakeAcceleration) * (float)elapsed.TotalSeconds;

                if (requestedBrakeAcceleration < OldBrakeAcceleration)
                {
                    brakeAcceleration = float.Min(float.Max(baseBrakeAcceleration, requestedBrakeAcceleration), 0);
                }
                else
                {
                    brakeAcceleration = float.Min(float.Min(baseBrakeAcceleration, requestedBrakeAcceleration), 0);
                }
            }

            OldRequestedBrakeAcceleration = float.Min(requestedBrakeAcceleration, 0);
            OldBrakeAcceleration = float.Min(brakeAcceleration, 0);

            float acceleration = float.Min(cruiseAcceleration, brakeAcceleration);
            if (float.Abs(speed) < 1e-3f && acceleration < 0)
            {
                acceleration = 0;
            }

            Acceleration = acceleration * direction;
        }
    }
}
