using TransportX.Dependency;
using TransportX.Input;
using TransportX.Physics;
using TransportX.Rendering;
using TransportX.Worlds;

namespace TransportX
{
    public class GameCreationInfo
    {
        public required PluginLoadContext Context { get; init; }

        public required IDXHost DXHost { get; init; }
        public required IDXClient DXClient { get; init; }
        public required PhysicsHost PhysicsHost { get; init; }

        public required Renderer Renderer { get; init; }

        public required TimeManager TimeManager { get; init; }
        public required InputManager InputManager { get; init; }
        public required Camera Camera { get; init; }

        public required WorldBase World { get; init; }
    }
}
