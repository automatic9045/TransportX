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

        event MovedEventHandler? Moved;

        Vector3 GetOffset(IWorldObject to) => WorldPose.GetOffset(to.WorldPose);
    }

    public delegate void MovedEventHandler(ChunkIndex chunkOffset);
}
