using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Spatial;

namespace TransportX
{
    public class WorldObject : IWorldObject
    {
        public static readonly WorldObject Origin = new();


        public WorldPose WorldPose { get; private set; }

        public event MovedEventHandler? Moved;

        public WorldObject(WorldPose worldPose)
        {
            Locate(worldPose);
        }

        public WorldObject() : this(WorldPose.Zero)
        {
        }

        protected ChunkIndex Locate(WorldPose worldPose)
        {
            if (WorldPose == worldPose) return ChunkIndex.Zero;

            WorldPose oldWorldPose = WorldPose;
            WorldPose = worldPose;

            ChunkIndex offset = WorldPose.Chunk - oldWorldPose.Chunk;
            Moved?.Invoke(offset);
            return offset;
        }

        protected ChunkIndex Locate(ChunkIndex chunkIndex, Pose pose)
        {
            WorldPose worldPose = new(chunkIndex, pose);
            return Locate(worldPose);
        }

        protected ChunkIndex Move(Pose delta)
        {
            WorldPose worldPose = delta * WorldPose;
            return Locate(worldPose);
        }
    }
}
