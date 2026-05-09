using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Worlds;

namespace TransportX
{
    public interface IAppFactory
    {
        IApp Create(AppHost host, IWorldInfo worldInfo);
    }
}
