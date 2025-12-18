using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Dependency;
using Bus.Common.Rendering;
using Bus.Common.Worlds;

namespace Bus.Common
{
    public interface IGameFactory
    {
        IGame Create(PluginLoadContext context, IDXHost dxHost, IDXClient dxClient, IWorldInfo worldInfo);
    }
}
