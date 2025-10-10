using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Scenery.Networks
{
    public class CombinedLaneKind : LaneKind
    {
        public IEnumerable<RootLaneKind> Children { get; }

        public CombinedLaneKind(string name, IEnumerable<RootLaneKind> children) : base(name)
        {
            Children = children;
        }

        public CombinedLaneKind(string name, params RootLaneKind[] children) : this(name, (IEnumerable<RootLaneKind>)children)
        {
        }

        public CombinedLaneKind(IEnumerable<RootLaneKind> children) : this(string.Join(" + ", children.Select(k => k.Name)), children)
        {
        }

        public CombinedLaneKind(params RootLaneKind[] children) : this((IEnumerable<RootLaneKind>)children)
        {
        }

        public static CombinedLaneKind operator +(CombinedLaneKind left, RootLaneKind right)
        {
            List<RootLaneKind> children = left.Children.ToList();
            children.Add(right);

            CombinedLaneKind result = new CombinedLaneKind(children);
            return result;
        }

        public static CombinedLaneKind operator +(RootLaneKind left, CombinedLaneKind right)
        {
            List<RootLaneKind> children = new List<RootLaneKind>() { left };
            children.AddRange(right.Children);

            CombinedLaneKind result = new CombinedLaneKind(children);
            return result;
        }

        public static CombinedLaneKind operator +(CombinedLaneKind left, CombinedLaneKind right)
        {
            List<RootLaneKind> children = left.Children.ToList();
            children.AddRange(right.Children);

            CombinedLaneKind result = new CombinedLaneKind(children);
            return result;
        }

        public static CombinedLaneKind operator -(CombinedLaneKind left, RootLaneKind right)
        {
            List<RootLaneKind> children = left.Children.ToList();
            if (!children.Remove(right))
            {
                throw new ArgumentException("右辺の値が左辺の子に含まれていません。", nameof(right));
            }

            CombinedLaneKind result = new CombinedLaneKind(children);
            return result;
        }

        public static CombinedLaneKind operator -(CombinedLaneKind left, CombinedLaneKind right)
        {
            List<RootLaneKind> children = left.Children.ToList();
            foreach (RootLaneKind rightChild in right.Children)
            {
                if (!children.Remove(rightChild))
                {
                    throw new ArgumentException("右辺の子の中で、左辺の子に含まれていない値があります。", nameof(right));
                }
            }

            CombinedLaneKind result = new CombinedLaneKind(children);
            return result;
        }

        public override int GetHashCode() => Children.GetHashCode();

        public override bool Equals(object? obj)
        {
            return obj is CombinedLaneKind other && Children.All(child => other.Children.Contains(child));
        }
    }
}
