using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Sample.Mathematics;

namespace TransportX.Sample.Vehicles.Doors
{
    internal class DoorAnimationProfile
    {
        public Diagram Diagram { get; }
        public PIDController PID { get; }
        public TimeSpan Duration { get; }

        public DoorAnimationProfile(Diagram diagram, PIDController pid, TimeSpan duration)
        {
            Diagram = diagram;
            PID = pid;
            Duration = duration;
        }
    }
}
