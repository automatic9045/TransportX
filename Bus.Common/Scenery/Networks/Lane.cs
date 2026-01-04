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


        public LaneKind Kind { get; }
        public FlowDirections Directions { get; }
        public Vector2 Position { get; }

        public Lane Opposition { get; }

        private Lane(LaneKind kind, FlowDirections directions, Vector2 position, Lane opposition)
        {
            Kind = kind;
            Directions = directions;
            Position = position;

            Opposition = opposition;
            Oppositions.TryAdd(this, opposition);
        }

        public Lane(LaneKind kind, FlowDirections directions, Vector2 position)
        {
            Kind = kind;
            Directions = directions;
            Position = position;

            Opposition = Oppositions.GetOrAdd(this, x =>
            {
                Lane opposition = new Lane(Kind, Directions.GetOpposition(), new Vector2(-Position.X, Position.Y), this);
                return opposition;
            });
        }

        public override string ToString() => $"{Position}: {Kind.Name}, {Directions}";

        public bool IsOppositeOf(Lane other)
        {
            return Kind == other.Kind
                && Directions.IsOppositeOf(other.Directions)
                && Position.X == -other.Position.X && Position.Y == other.Position.Y;
        }
    }
}
