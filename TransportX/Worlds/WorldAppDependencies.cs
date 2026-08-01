using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Audio;
using TransportX.Cameras;
using TransportX.Physics;
using TransportX.Rendering.Backend;
using TransportX.Rendering.Pipelines;

namespace TransportX.Worlds
{
    public readonly struct WorldAppDependencies
    {
        public required IAppHost Host { get; init; }

        public required GraphicsHost GraphicsHost { get; init; }
        public required GraphicsClient GraphicsClient { get; init; }
        public required AudioClient AudioClient { get; init; }
        public required PhysicsHost PhysicsHost { get; init; }

        public required ViewpointSet Viewpoints { get; init; }
        public required IRenderer Renderer { get; init; }

        public required TimeManager UpdateTimeManager { get; init; }
        public required TimeManager RenderTimeManager { get; init; }

        public required WorldBase World { get; init; }
    }
}
