using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Dependency;
using TransportX.Rendering;
using TransportX.Worlds;

namespace TransportX
{
    public interface IGameFactory
    {
        IGame Create(PluginLoadContext context, IDXHost dxHost, IDXClient dxClient, IWorldInfo worldInfo);
    }
}
