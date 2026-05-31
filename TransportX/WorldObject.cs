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
        public virtual Vector3 Velocity => Vector3.Zero;

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

            WorldPose = worldPose;
            Moved?.Invoke(worldPose.NormalizedOffset);

            return worldPose.NormalizedOffset;
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

        public Vector3 GetOffset(IWorldObject to) => ((IWorldObject)this).GetOffset(to);
    }
}
