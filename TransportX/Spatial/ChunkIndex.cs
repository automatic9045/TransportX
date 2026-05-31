using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Spatial
{
    public readonly record struct ChunkIndex
    {
        public static readonly ChunkIndex Zero = new();


        public int X { get; }
        public int Z { get; }
        public Vector3 Position { get; }
        public Pose Pose => new(Position);
        public Pose PoseInverse => new(-Position);

        public readonly bool IsZero => X == 0 && Z == 0;

        public ChunkIndex() : this(0, 0)
        {
        }

        public ChunkIndex(int deltaX, int deltaZ)
        {
            X = deltaX;
            Z = deltaZ;

            Position = Chunk.Size * new Vector3(deltaX, 0, deltaZ);
        }

        public static ChunkIndex operator +(ChunkIndex left, ChunkIndex right) => new(left.X + right.X, left.Z + right.Z);
        public static ChunkIndex operator -(ChunkIndex left, ChunkIndex right) => new(left.X - right.X, left.Z - right.Z);
    }
}
