using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Scenery.Networks
{
    public class Lane
    {
        private static readonly ConcurrentDictionary<Lane, Lane> Oppositions = [];


        public LaneTrafficGroup AllowedTraffic { get; }
        public FlowDirections Directions { get; }
        public Vector2 Position { get; }

        public Lane Opposition { get; }

        private Lane(LaneTrafficGroup allowedTraffic, FlowDirections directions, Vector2 position, Lane opposition)
        {
            AllowedTraffic = allowedTraffic;
            Directions = directions;
            Position = position;

            Opposition = opposition;
            Oppositions.TryAdd(this, opposition);
        }

        public Lane(LaneTrafficGroup allowedTraffic, FlowDirections directions, Vector2 position)
        {
            AllowedTraffic = allowedTraffic;
            Directions = directions;
            Position = position;

            Opposition = Oppositions.GetOrAdd(this, x =>
            {
                Lane opposition = new Lane(AllowedTraffic, Directions.GetOpposition(), new Vector2(-Position.X, Position.Y), this);
                return opposition;
            });
        }

        public override string ToString() => $"{Position}: {AllowedTraffic}, {Directions}";

        public bool IsOppositeOf(Lane other)
        {
            return AllowedTraffic == other.AllowedTraffic
                && Directions.IsOppositeOf(other.Directions)
                && Position.X == -other.Position.X && Position.Y == other.Position.Y;
        }
    }
}
