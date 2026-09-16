using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Rendering.Pipelines
{
    public readonly struct RendererOptions
    {
        public required int MaxAnisotropy { get; init => field = int.Clamp(value, 1, 16); }
        public required int DrawChunkCount { get; init; }
        public required ShadowOptions ShadowOptions { get; init; }
    }
}
