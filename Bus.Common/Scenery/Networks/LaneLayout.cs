using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Scenery.Networks
{
    public class LaneLayout
    {
        public IReadOnlyList<Lane> Lanes { get; }

        public LaneLayout(IReadOnlyList<Lane> lanes)
        {
            Lanes = lanes;
        }

        public LaneLayout(params Lane[] lanes) : this((IReadOnlyList<Lane>)lanes)
        {
        }

        public bool CanConnectTo(LaneLayout other)
        {
            if (Lanes.Count != other.Lanes.Count) return false;

            for (int i = 0; i < Lanes.Count; i++)
            {
                Lane from = Lanes[i];
                Lane to = other.Lanes[Lanes.Count - 1 - i];

                if (!from.IsOpposite(to)) return false;
            }

            return true;
        }

        public LaneLayout CreateCopy()
        {
            Lane[] newLanes = Lanes
                .Select(lane => lane.CreateCopy())
                .ToArray();

            return new LaneLayout(newLanes);
        }

        public LaneLayout CreateOpposition()
        {
            Lane[] newLanes = Lanes
                .Reverse()
                .Select(lane => lane.CreateOpposite())
                .ToArray();

            return new LaneLayout(newLanes);
        }

        public LanePin[] CreatePins(NetworkElement parent)
        {
            return Lanes.Select(lane => new LanePin(parent, lane)).ToArray();
        }
    }
}
