using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Rendering.Pipelines.Shadows;

namespace TransportX.Rendering.Pipelines
{
    public readonly struct RendererOptions
    {
        public required int DrawChunkCount { get; init; }
        public required ShadowOptions ShadowOptions { get; init; }
    }
}
