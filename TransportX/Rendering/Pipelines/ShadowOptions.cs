using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Rendering.Pipelines
{
    public readonly struct ShadowOptions
    {
        public required int DrawChunkCount { get; init; }
        public required int Resolution { get; init; }
    }
}
