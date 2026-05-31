using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Spatial
{
    public readonly record struct WorldPose
    {
        public static readonly WorldPose Zero = new(ChunkIndex.Zero, Pose.Identity);


        public ChunkIndex Chunk { get; }
        public Pose Pose { get; }

        public ChunkIndex NormalizedOffset { get; }
        public Vector3 WorldPosition => Chunk.Position + Pose.Position;

        public WorldPose(ChunkIndex chunk, Pose pose)
        {
            int dx = GetChunkDelta(pose.Position.X);
            int dz = GetChunkDelta(pose.Position.Z);

            Chunk = chunk + new ChunkIndex(dx, dz);
            Pose = new Pose(pose.Position - new Vector3(dx, 0, dz) * Spatial.Chunk.Size, pose.Orientation);

            NormalizedOffset = new ChunkIndex(dx, dz);


            static int GetChunkDelta(float delta)
            {
                return (int)float.Floor(delta / Spatial.Chunk.Size);
            }
        }

        public WorldPose(ChunkIndex chunk, SixDoF position) : this(chunk, position.ToPose())
        {
        }

        public static WorldPose operator *(WorldPose a, Pose b) => new(a.Chunk, a.Pose * b);
        public static WorldPose operator *(Pose a, WorldPose b) => new(b.Chunk, a * b.Pose);
        public static WorldPose operator +(WorldPose a, ChunkIndex b) => new(a.Chunk + b, a.Pose);
        public static WorldPose operator -(WorldPose a, ChunkIndex b) => new(a.Chunk - b, a.Pose);

        public WorldPose ChangePose(Pose newPose) => new(Chunk, newPose);
        public Vector3 GetOffset(WorldPose to) => to.Pose.Position - Pose.Position + (to.Chunk - Chunk).Position;
    }
}
