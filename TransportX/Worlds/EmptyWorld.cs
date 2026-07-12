using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Collections;
using TransportX.Dependency;

namespace TransportX.Worlds
{
    internal class EmptyWorld : WorldBase
    {
        public override IModelCollection Models { get; } = new ModelCollection();
        public override ISoundCollection Sounds { get; } = new SoundCollection();

        public EmptyWorld(WorldBuilder builder) : base(new PluginLoadContext(typeof(EmptyWorld).Assembly.Location), builder)
        {
            AppContext.Children.Add(WorldContext);
        }
    }
}
