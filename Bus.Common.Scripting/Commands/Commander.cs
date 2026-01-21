using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Scripting.Commands
{
    public class Commander
    {
        public ScriptWorld World { get; }

        public Background Background { get; }
        public Debug Debug { get; }
        public Models Models { get; }
        public Plates Plates { get; }
        public Network Network { get; }
        public Triggers Triggers { get; }
        public Avatars Avatars { get; }

        internal Commander(ScriptWorld world)
        {
            World = world;

            Background = new Background(world);
            Debug = new Debug(world);
            Models = new Models(world);
            Plates = new Plates(world);
            Network = new Network(world);
            Triggers = new Triggers(world);
            Avatars = new Avatars(world);
        }

        private protected Commander(Commander parent)
        {
            World = parent.World;

            Background = parent.Background;
            Debug = parent.Debug;
            Models = parent.Models;
            Plates = parent.Plates;
            Network = parent.Network;
            Triggers = parent.Triggers;
            Avatars = parent.Avatars;
        }

        internal void Dispose()
        {
            Triggers.Dispose();
        }
    }
}
