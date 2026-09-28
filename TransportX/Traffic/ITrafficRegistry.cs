using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Spatial;

namespace TransportX.Traffic
{
    public interface ITrafficRegistry
    {
        IReadOnlyList<ITrafficEntity> Entities { get; }

        IReadOnlyList<ITrafficEntity> GetEntitiesInChunk(ChunkIndex chunk);
    }
}
