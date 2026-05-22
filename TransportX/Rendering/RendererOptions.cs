using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Rendering.Shadows;

namespace TransportX.Rendering
{
    public readonly struct RendererOptions
    {
        public required ShadowOptions ShadowOptions { get; init; }
    }
}
