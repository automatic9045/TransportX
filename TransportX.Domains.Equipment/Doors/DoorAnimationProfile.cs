using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Mathematics;

namespace TransportX.Domains.Equipment.Doors
{
    public class DoorAnimationProfile
    {
        public Curve Curve { get; }
        public PidController Pid { get; }
        public TimeSpan Duration { get; }

        public DoorAnimationProfile(Curve curve, PidController pid, TimeSpan duration)
        {
            Curve = curve;
            Pid = pid;
            Duration = duration;
        }
    }
}
