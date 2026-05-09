using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Dependency;

namespace TransportX
{
    public class AppHost
    {
        public required PluginLoadContext Context { get; init; }
        public required Platform Platform { get; init; }

        public event Action? ReloadRequested;

        public void RequestReload()
        {
            ReloadRequested?.Invoke();
        }
    }
}
