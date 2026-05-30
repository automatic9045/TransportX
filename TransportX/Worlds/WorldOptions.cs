using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Worlds
{
    public readonly struct WorldOptions
    {
        public required int SimulationChunkCount { get; init; }
    }
}
