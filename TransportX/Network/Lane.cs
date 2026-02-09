using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Network
{
    public class Lane
    {
        private static readonly ConcurrentDictionary<Lane, Lane> Oppositions = [];


        public LaneTrafficGroup AllowedTraffic { get; }
        public FlowDirections Directions { get; }
        public Vector2 Position { get; }
        public LaneWidth Width { get; }

        public Lane Opposition { get; }

        private Lane(LaneTrafficGroup allowedTraffic, FlowDirections directions, Vector2 position, LaneWidth width, Lane opposition)
        {
            AllowedTraffic = allowedTraffic;
            Directions = directions;
            Position = position;
            Width = width;

            Opposition = opposition;
            Oppositions.TryAdd(this, opposition);
        }

        public Lane(LaneTrafficGroup allowedTraffic, FlowDirections directions, Vector2 position, LaneWidth width)
        {
            AllowedTraffic = allowedTraffic;
            Directions = directions;
            Position = position;
            Width = width;

            Opposition = Oppositions.GetOrAdd(this, x =>
            {
                Lane opposition = new(AllowedTraffic, Directions.GetOpposition(), new Vector2(-Position.X, Position.Y), LaneWidth.Opposition(Width), this);
                return opposition;
            });
        }

        public override string ToString() => $"{Position}: {AllowedTraffic}, {Directions}";

        public bool IsOppositeOf(Lane other)
        {
            return AllowedTraffic == other.AllowedTraffic
                && Directions.IsOppositeOf(other.Directions)
                && Position.X == -other.Position.X && Position.Y == other.Position.Y
                && Width.Left == other.Width.Right && Width.Right == other.Width.Left;
        }
    }
}
