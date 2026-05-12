using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Dependency;

namespace TransportX
{
    public interface IAppHost
    {
        PluginLoadContext Context { get; }
        Platform Platform { get; }
        AppReference CurrentReference { get; }

        void RequestPushApp(AppReference appReference, IAppParameters parameters);
        void RequestPopApp();
        void RequestLoadApp(AppReference appReference, IAppParameters parameters);
    }
}
