using System;
using System.Collections.Generic;
using System.Text;

using TransportX.Dependency;
using TransportX.Rendering;

namespace TransportX
{
    public readonly struct RuntimeHost
    {
        public required PluginLoadContext Context { get; init; }

        public required Platform Platform { get; init; }
        public required IDXHost DXHost { get; init; }
        public required IDXClient DXClient { get; init; }
    }
}
