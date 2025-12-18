using Bus.Common.Dependency;
using Bus.Common.Input;
using Bus.Common.Physics;
using Bus.Common.Rendering;
using Bus.Common.Worlds;

namespace Bus.Common
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
