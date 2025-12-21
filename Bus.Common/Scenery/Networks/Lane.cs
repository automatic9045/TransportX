using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Scenery.Networks
{
    public sealed class Lane
    {
        public LaneKind Kind { get; }
        public FlowDirections Directions { get; }
        public Vector2 Position { get; }

        public Lane(LaneKind kind, FlowDirections directions, Vector2 position)
        {
            Kind = kind;
            Directions = directions;
            Position = position;
        }

        public override string ToString() => $"{Position}: {Kind.Name}, {Directions}";

        public bool IsOpposite(Lane other)
        {
            return Kind == other.Kind
                && Directions.IsOppositeOf(other.Directions)
                && Position.X == -other.Position.X && Position.Y == other.Position.Y;
        }

        public Lane CreateCopy()
        {
            return new Lane(Kind, Directions, Position);
        }

        public Lane CreateOpposite()
        {
            return new Lane(Kind, Directions.GetOpposition(), new Vector2(-Position.X, Position.Y));
        }
    }
}
