using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Scenery.Networks
{
    public abstract class LanePath
    {
        public LanePin From { get; }
        public LanePin To { get; }
        public double Length { get; }

        protected LanePath(LanePin from, LanePin to, double length)
        {
            From = from;
            To = to;
            Length = length;
        }

        public abstract Matrix4x4 GetTransform(double at);
    }
}
