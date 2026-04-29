using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Spatial;

namespace TransportX
{
    public class LocatableObject : ILocatable
    {
        public static readonly LocatableObject Origin = new();


        public WorldPose WorldPose { get; private set; }
        public virtual Vector3 Velocity => Vector3.Zero;

        public event Action<ChunkOffset>? Moved;

        public LocatableObject(WorldPose worldPose)
        {
            Locate(worldPose);
        }

        public LocatableObject() : this(WorldPose.Zero)
        {
        }

        protected ChunkOffset Locate(WorldPose worldPose)
        {
            if (WorldPose == worldPose) return ChunkOffset.Identity;

            WorldPose = worldPose;
            Moved?.Invoke(worldPose.NormalizedOffset);

            return worldPose.NormalizedOffset;
        }

        protected ChunkOffset Locate(ILocatable attachTo, Pose pose)
        {
            WorldPose worldPose = new(attachTo.WorldPose.ChunkX, attachTo.WorldPose.ChunkZ, pose);
            return Locate(worldPose);
        }

        protected ChunkOffset Locate(ILocatable attachTo)
        {
            return Locate(attachTo.WorldPose);
        }

        protected ChunkOffset Move(Pose delta)
        {
            WorldPose worldPose = new(WorldPose.ChunkX, WorldPose.ChunkZ, delta * WorldPose.Pose);
            return Locate(worldPose);
        }

        public ChunkOffset GetChunkOffset(ILocatable to) => ((ILocatable)this).GetChunkOffset(to);
        public Vector3 GetOffset(ILocatable to) => ((ILocatable)this).GetOffset(to);
    }
}
