using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Scenery.Networks
{
    public abstract class LaneKind
    {
        public string Name { get; }

        private protected LaneKind(string name)
        {
            Name = name;
        }

        public static CombinedLaneKind operator +(LaneKind left, LaneKind right)
        {
            if (left is RootLaneKind rootLeft)
            {
                if (right is RootLaneKind rootRight) return rootLeft + rootRight;
                if (right is CombinedLaneKind combinedRight) return rootLeft + combinedRight;
            }
            else if (left is CombinedLaneKind combinedLeft)
            {
                if (right is RootLaneKind rootRight) return combinedLeft + rootRight;
                if (right is CombinedLaneKind combinedRight) return combinedLeft + combinedRight;
            }

            throw new NotSupportedException();
        }

        public override string ToString() => Name;
    }
}
