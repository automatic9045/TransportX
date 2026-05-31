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

        event MovedEventHandler? Moved;

        sealed Vector3 GetOffset(IWorldObject to) => WorldPose.GetOffset(to.WorldPose);
    }

    public delegate void MovedEventHandler(ChunkIndex chunkOffset);
}
