using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Scenery.Networks
{
    public abstract class LaneKind
    {
        public static readonly RootLaneKind Pedestrians = new RootLaneKind("歩行者");
        public static readonly RootLaneKind Cars = new RootLaneKind("自動車");


        public string Name { get; }

        private protected LaneKind(string name)
        {
            Name = name;
        }

        public override string ToString() => Name;
    }
}
