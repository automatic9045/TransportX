using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Spatial;

namespace TransportX
{
    public interface IWorldObject
    {
        WorldPose WorldPose { get; }
        Vector3 Velocity { get; }

        event Action<ChunkOffset>? Moved;

        sealed ChunkOffset GetChunkOffset(IWorldObject to) => WorldPose.GetChunkOffset(to.WorldPose);
        sealed Vector3 GetOffset(IWorldObject to) => WorldPose.GetOffset(to.WorldPose);
    }
}
