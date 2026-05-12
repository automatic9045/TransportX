using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Dependency;

namespace TransportX.Player
{
    internal class AppHost : IAppHost
    {
        public required PluginLoadContext Context { get; init; }
        public required Platform Platform { get; init; }
        public required AppReference CurrentReference { get; init; }

        public event Action<AppReference, IAppParameters>? LoadRequested;

        public void RequestPushApp(AppReference appReference, IAppParameters parameters)
        {
            throw new NotImplementedException();
        }

        public void RequestPopApp()
        {
            throw new NotImplementedException();
        }

        public void RequestLoadApp(AppReference appReference, IAppParameters parameters)
        {
            LoadRequested?.Invoke(appReference, parameters);
        }
    }
}
