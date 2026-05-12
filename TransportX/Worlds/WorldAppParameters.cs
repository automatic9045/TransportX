using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Worlds
{
    public readonly struct WorldAppParameters : IAppParameters
    {
        public IWorldInfo WorldInfo { get; }

        public WorldAppParameters(IWorldInfo worldInfo)
        {
            WorldInfo = worldInfo;
        }
    }
}
