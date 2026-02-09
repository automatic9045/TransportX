using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Extensions.Mathematics
{
    public abstract class PoseCurveBase
    {
        public Pose From { get; }
        public Pose To { get; }

        public abstract float Length { get; }

        protected PoseCurveBase(Pose from, Pose to)
        {
            From = from;
            To = to;
        }

        public abstract Pose GetPose(float s);
    }
}
