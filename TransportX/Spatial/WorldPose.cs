using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Spatial
{
    public readonly struct WorldPose
    {
        public static readonly WorldPose Zero = new(0, 0, Pose.Identity);


        public int ChunkX { get; }
        public int ChunkZ { get; }
        public Pose Pose { get; }

        public ChunkOffset NormalizedOffset { get; }
        public Vector3 WorldPosition => Pose.Position + new Vector3(ChunkX, 0, ChunkZ) * Chunk.Size;

        public WorldPose(int chunkX, int chunkZ, Pose pose)
        {
            int dx = GetChunkDelta(pose.Position.X);
            int dz = GetChunkDelta(pose.Position.Z);

            pose = new(pose.Position - new Vector3(dx, 0, dz) * Chunk.Size, pose.Orientation);

            chunkX += dx;
            chunkZ += dz;

            ChunkX = chunkX;
            ChunkZ = chunkZ;
            Pose = pose;

            NormalizedOffset = new ChunkOffset(dx, dz);


            static int GetChunkDelta(float delta)
            {
                return (int)float.Floor(delta / Chunk.Size);
            }
        }

        public WorldPose(int chunkX, int chunkZ, SixDoF position) : this(chunkX, chunkZ, position.ToPose())
        {
        }

        public static bool operator ==(WorldPose a, WorldPose b) => a.ChunkX == b.ChunkX && a.ChunkZ == b.ChunkZ && a.Pose == b.Pose;
        public static bool operator !=(WorldPose a, WorldPose b) => a.ChunkX != b.ChunkX || a.ChunkZ != b.ChunkZ || a.Pose != b.Pose;
        public static WorldPose operator *(WorldPose a, Pose b) => new(a.ChunkX, a.ChunkZ, a.Pose * b);
        public static WorldPose operator *(Pose a, WorldPose b) => new(b.ChunkX, b.ChunkZ, a * b.Pose);
        public static WorldPose operator +(WorldPose a, ChunkOffset b) => new(a.ChunkX + b.DeltaX, a.ChunkZ + b.DeltaZ, a.Pose);
        public static WorldPose operator -(WorldPose a, ChunkOffset b) => new(a.ChunkX - b.DeltaX, a.ChunkZ - b.DeltaZ, a.Pose);

        public override bool Equals(object? obj) => obj is WorldPose worldPose && this == worldPose;
        public override int GetHashCode() => HashCode.Combine(ChunkX, ChunkZ, Pose);

        public WorldPose ChangePose(Pose newPose) => new(ChunkX, ChunkZ, newPose);
        public ChunkOffset GetChunkOffset(WorldPose to) => new(to.ChunkX - ChunkX, to.ChunkZ - ChunkZ);
        public Vector3 GetOffset(WorldPose to) => to.Pose.Position - Pose.Position + GetChunkOffset(to).Position;
    }
}
