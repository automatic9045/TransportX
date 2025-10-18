using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Physics
{
    public readonly struct ColliderGroupHandle
    {
        private static int NextId = 1;
        public static readonly ColliderGroupHandle None = default;


        public int Id { get; } = 0;

        public ColliderGroupHandle(int id)
        {
            Id = id;
        }


        public static ColliderGroupHandle NewGroup()
        {
            return new ColliderGroupHandle(NextId++);
        }

        public static bool operator ==(ColliderGroupHandle left, ColliderGroupHandle right) => left.Id == right.Id;
        public static bool operator !=(ColliderGroupHandle left, ColliderGroupHandle right) => !(left == right);

        public override bool Equals(object? obj) => obj is ColliderGroupHandle handle && this == handle;
        public override int GetHashCode() => HashCode.Combine(Id);
    }
}
