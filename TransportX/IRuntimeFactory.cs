using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Worlds;

namespace TransportX
{
    public interface IRuntimeFactory
    {
        IRuntime Create(RuntimeHost runtimeHost, IWorldInfo worldInfo);
    }
}
