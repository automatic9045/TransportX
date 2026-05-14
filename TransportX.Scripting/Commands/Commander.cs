using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Scripting.Commands
{
    public class Commander
    {
        public ScriptWorld World { get; }

        public Avatar Avatar { get; }
        public Background Background { get; }
        public Camera Camera { get; }
        public Chunks Chunks { get; }
        public Components Components { get; }
        public Debug Debug { get; }
        public DirectionalLight DirectionalLight { get; }
        public WorldEnvironment Environment { get; }
        public Models Models { get; }
        public Network Network { get; }
        public Triggers Triggers { get; }

        internal Commander(ScriptWorld world)
        {
            World = world;

            Avatar = new Avatar(world);
            Background = new Background(world);
            Camera = new Camera(world);
            Chunks = new Chunks(world);
            Components = new Components(world);
            Debug = new Debug(world);
            DirectionalLight = new DirectionalLight(world);
            Environment = new WorldEnvironment(world);
            Models = new Models(world);
            Network = new Network(world);
            Triggers = new Triggers(world);
        }

        private protected Commander(Commander parent)
        {
            World = parent.World;

            Avatar = parent.Avatar;
            Background = parent.Background;
            Camera = parent.Camera;
            Chunks = parent.Chunks;
            Components = parent.Components;
            Debug = parent.Debug;
            DirectionalLight = parent.DirectionalLight;
            Environment = parent.Environment;
            Models = parent.Models;
            Network = parent.Network;
            Triggers = parent.Triggers;
        }

        internal void Dispose()
        {
            Triggers.Dispose();
        }

        public T Component<T>() where T : class, IWorldInstantiable<T>, IComponentCommand
        {
            return Components.Get<T>();
        }
    }
}
