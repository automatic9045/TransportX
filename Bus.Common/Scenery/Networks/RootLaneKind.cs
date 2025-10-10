using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Scenery.Networks
{
    public class RootLaneKind : LaneKind
    {
        public RootLaneKind(string name) : base(name)
        {
        }

        public static CombinedLaneKind operator +(RootLaneKind left, RootLaneKind right)
        {
            CombinedLaneKind result = new CombinedLaneKind(left, right);
            return result;
        }
    }
}
