using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Dependency;
using TransportX.Rendering;

namespace TransportX.Worlds
{
    internal class EmptyWorld : WorldBase
    {
        public override IModelCollection Models { get; } = new ModelCollection();

        public EmptyWorld(WorldBuilder builder) : base(new PluginLoadContext(typeof(EmptyWorld).Assembly.Location), builder)
        {
            GameContext.Children.Add(WorldContext);
        }
    }
}
