using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Network
{
    public class LaneTrafficGroup : IEquatable<LaneTrafficGroup>
    {
        public static LaneTrafficGroup operator |(LaneTrafficGroup left, LaneTrafficType right)
        {
            IEnumerable<LaneTrafficType> types = Enumerable.Distinct([.. left.Types, right]);
            return new LaneTrafficGroup(types);
        }

        public static LaneTrafficGroup operator |(LaneTrafficGroup left, LaneTrafficGroup right)
        {
            IEnumerable<LaneTrafficType> types = Enumerable.Distinct([.. left.Types, .. right.Types]);
            return new LaneTrafficGroup(types);
        }

        public static LaneTrafficGroup operator -(LaneTrafficGroup left, LaneTrafficType right)
        {
            IEnumerable<LaneTrafficType> types = left.Types.Where(t => t != right);
            return new LaneTrafficGroup(types);
        }

        public static LaneTrafficGroup operator -(LaneTrafficGroup left, LaneTrafficGroup right)
        {
            IEnumerable<LaneTrafficType> types = left.Types.Except(right.Types);
            return new LaneTrafficGroup(types);
        }

        public static LaneTrafficGroup operator &(LaneTrafficGroup left, LaneTrafficGroup right)
        {
            IEnumerable<LaneTrafficType> types = left.Types.Intersect(right.Types);
            return new LaneTrafficGroup(types);
        }


        public IEnumerable<LaneTrafficType> Types { get; }

        public LaneTrafficGroup(IEnumerable<LaneTrafficType> types)
        {
            Types = types.Distinct().ToArray();
        }

        public LaneTrafficGroup(params LaneTrafficType[] types) : this((IEnumerable<LaneTrafficType>)types)
        {
        }

        public bool Contains(LaneTrafficType type)
        {
            return Types.Contains(type);
        }

        public override string ToString()
        {
            return string.Join('|', Types.Select(type => type.Name));
        }

        public bool Equals(LaneTrafficGroup? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            HashSet<LaneTrafficType> set = [.. Types];
            return set.SetEquals(other.Types);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as LaneTrafficGroup);
        }

        public override int GetHashCode()
        {
            int hash = 0;
            foreach (LaneTrafficType type in Types) hash ^= type.GetHashCode();

            return hash;
        }
    }
}
