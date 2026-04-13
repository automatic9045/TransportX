using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Sample.LV290.Mathematics;

namespace TransportX.Sample.LV290.Vehicles.Doors
{
    internal class DoorAnimator
    {
        private readonly DoorAnimationProfile OpenProfile;
        private readonly DoorAnimationProfile CloseProfile;

        private readonly float Restitution0;
        private readonly float Restitution1;

        private TimeSpan Time = TimeSpan.Zero;

        private bool OldIsOpen = false;
        public bool IsOpen { get; set; } = false;

        public float Progress { get; private set; } = 0;
        public float Velocity { get; private set; } = 0;
        public float OpenRate { get; private set; } = 0;

        public DoorAnimator(DoorAnimationProfile openProfile, DoorAnimationProfile closeProfile, float restitution0, float restitution1)
        {
            OpenProfile = openProfile;
            CloseProfile = closeProfile;

            Restitution0 = restitution0;
            Restitution1 = restitution1;
        }

        public void Tick(TimeSpan elapsed)
        {
            DoorAnimationProfile profile = IsOpen ? OpenProfile : CloseProfile;

            if (OldIsOpen != IsOpen)
            {
                TimeSpan oldDuration = OldIsOpen ? OpenProfile.Duration : CloseProfile.Duration;

                float newProgress = oldDuration <= TimeSpan.Zero ? 0 : (float)(Time.TotalSeconds / oldDuration.TotalSeconds);
                newProgress = float.Clamp(newProgress, 0, 1);

                Time = TimeSpan.FromSeconds(profile.Duration.TotalSeconds * newProgress);

                profile.PID.Reset();
                OldIsOpen = IsOpen;
            }

            TimeSpan duration = profile.Duration;

            if (IsOpen)
            {
                Time += elapsed;
                if (duration < Time)
                {
                    Time = duration;
                }
            }
            else
            {
                Time -= elapsed;
                if (Time < TimeSpan.Zero)
                {
                    Time = TimeSpan.Zero;
                }
            }

            Progress = duration <= TimeSpan.Zero ? 0 : (float)(Time.TotalSeconds / duration.TotalSeconds);
            float targetOpenRate = profile.Diagram.GetValue(Progress);

            float error = targetOpenRate - OpenRate;
            float acceleration = profile.PID.Next(error, elapsed);

            float dt = (float)elapsed.TotalSeconds;
            float velocity = Velocity + acceleration * dt;
            float openRate = OpenRate + velocity * dt;

            if (openRate < 0)
            {
                openRate *= -Restitution0;
                velocity *= -Restitution0;
                profile.PID.Reset();
            }
            else if (1 < openRate)
            {
                openRate = 1 - (openRate - 1) * Restitution1;
                velocity *= -Restitution1;
                profile.PID.Reset();
            }

            Velocity = velocity;
            OpenRate = openRate;
        }
    }
}
