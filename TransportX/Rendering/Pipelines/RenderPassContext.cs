using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;

using TransportX.Cameras;
using TransportX.Rendering.Backend;
using TransportX.Spatial;

namespace TransportX.Rendering.Pipelines
{
    public readonly struct RenderPassContext
    {
        public required RenderSurface Surface { get; init; }
        public required RendererOptions Options { get; init; }
        public required ICamera Camera { get; init; }
        public required ViewContext ViewContext { get; init; }
        public required RenderPassFlags Flags { get; init; }
        public required RenderPassOutputMode OutputMode { get; init; }
        public required SizeI ViewportSize { get; init; }
        public required TimeSpan Elapsed { get; init; }

        public RenderPassContext()
        {
        }
    }
}
