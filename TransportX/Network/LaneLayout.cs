using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Network
{
    public class LaneLayout
    {
        private static readonly ConcurrentDictionary<LaneLayout, LaneLayout> Oppositions = [];


        public IReadOnlyList<Lane> Lanes { get; }

        public LaneLayout Opposition { get; }

        private LaneLayout(IReadOnlyList<Lane> lanes, LaneLayout opposition)
        {
            Lanes = lanes;

            Opposition = opposition;
            Oppositions.TryAdd(this, opposition);
        }

        public LaneLayout(IReadOnlyList<Lane> lanes)
        {
            Lanes = lanes;

            Opposition = Oppositions.GetOrAdd(this, x =>
            {
                Lane[] oppositeLanes = Lanes
                    .Reverse()
                    .Select(lane => lane.Opposition)
                    .ToArray();

                LaneLayout opposition = new LaneLayout(oppositeLanes, this);
                return opposition;
            });
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

                if (!from.IsOppositeOf(to)) return false;
            }

            return true;
        }

        public LanePin[] CreatePins(NetworkPort port)
        {
            LanePin[] pins = new LanePin[Lanes.Count];
            for (int i = 0; i < pins.Length; i++)
            {
                pins[i] = new LanePin(port, i);
            }

            return pins;
        }
    }
}
