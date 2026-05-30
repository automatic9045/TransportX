using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Cameras;
using TransportX.Input;
using TransportX.Physics;
using TransportX.Rendering.Backend;
using TransportX.Rendering.Pipelines;

namespace TransportX.Worlds
{
    public readonly struct WorldAppDependencies
    {
        public required IAppHost Host { get; init; }

        public required DXHost DXHost { get; init; }
        public required DXClient DXClient { get; init; }
        public required PhysicsHost PhysicsHost { get; init; }

        public required Renderer Renderer { get; init; }

        public required TimeManager UpdateTimeManager { get; init; }
        public required TimeManager RenderTimeManager { get; init; }
        public required InputManager InputManager { get; init; }
        public required Camera Camera { get; init; }

        public required WorldBase World { get; init; }
    }
}
