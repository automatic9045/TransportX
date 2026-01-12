using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Scenery.Networks
{
    public class LaneTrafficType
    {
        public static LaneTrafficType Empty() => new LaneTrafficType(string.Empty);

        public static LaneTrafficGroup operator |(LaneTrafficType left, LaneTrafficType right)
        {
            return new LaneTrafficGroup([left, right]);
        }


        public string Name { get; }

        public LaneTrafficType(string name)
        {
            Name = name;
        }

        public override string ToString() => Name;
    }
}
