using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Scenery.Networks
{
    public sealed class LanePin
    {
        public LaneKind Kind { get; }
        public FlowDirection Direction { get; }
        public Vector2 Position { get; }

        private readonly List<LanePath> SourcePathsKey = new List<LanePath>();
        public IReadOnlyList<LanePath> SourcePaths => SourcePathsKey;

        private readonly List<LanePath> DestPathsKey = new List<LanePath>();
        public IReadOnlyList<LanePath> DestPaths => DestPathsKey;

        public LanePin(LaneKind kind, FlowDirection direction, Vector2 position)
        {
            Kind = kind;
            Direction = direction;
            Position = position;
        }

        public override string ToString() => $"{Position}: {Kind.Name}, {Direction}";

        public bool IsOpposite(LanePin other)
        {
            return Kind == other.Kind && (int)Direction + (int)other.Direction == 0 && Position.Y == other.Position.Y && Position.X == -other.Position.X;
        }

        public LanePin CreateCopy()
        {
            return new LanePin(Kind, Direction, Position);
        }

        public LanePin CreateOpposite()
        {
            return new LanePin(Kind, (FlowDirection)(-(int)Direction), new Vector2(-Position.X, Position.Y));
        }

        public void Wire(LanePath path)
        {
            bool isWired = false;

            if (path.From == this)
            {
                isWired = true;
                SourcePathsKey.Add(path);
            }

            if (path.To == this)
            {
                isWired = true;
                DestPathsKey.Add(path);
            }

            if (!isWired) throw new ArgumentException("開始点、終了点のどちらもこの端子ではありません。");
        }


        public enum FlowDirection
        {
            Out = -1,
            InOut = 0,
            In = 1,
        }
    }
}
