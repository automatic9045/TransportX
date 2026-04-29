using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Spatial
{
    public readonly struct ChunkOffset
    {
        public static readonly ChunkOffset Identity = new ChunkOffset();


        public int DeltaX { get; }
        public int DeltaZ { get; }
        public Vector3 Position { get; }
        public Pose Pose => new(Position);
        public Pose PoseInverse => new(-Position);

        public readonly bool IsZero => DeltaX == 0 && DeltaZ == 0;

        public ChunkOffset() : this(0, 0)
        {
        }

        public ChunkOffset(int deltaX, int deltaZ)
        {
            DeltaX = deltaX;
            DeltaZ = deltaZ;

            Position = Chunk.Size * new Vector3(deltaX, 0, deltaZ);
        }

        public static bool operator ==(ChunkOffset left, ChunkOffset right) => left.DeltaX == right.DeltaX && left.DeltaZ == right.DeltaZ;
        public static bool operator !=(ChunkOffset left, ChunkOffset right) => !(left == right);
        public static ChunkOffset operator +(ChunkOffset left, ChunkOffset right) => new ChunkOffset(left.DeltaX + right.DeltaX, left.DeltaZ + right.DeltaZ);
        public static ChunkOffset operator -(ChunkOffset left, ChunkOffset right) => new ChunkOffset(left.DeltaX - right.DeltaX, left.DeltaZ - right.DeltaZ);

        public override bool Equals(object? obj) => obj is ChunkOffset offset && this == offset;
        public override int GetHashCode() => HashCode.Combine(DeltaX, DeltaZ);
    }
}
