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
        IApp Create(IAppHost host, IAppParameters parameters);
    }

    public interface IAppFactory<TParameters> : IAppFactory where TParameters : IAppParameters
    {
        IApp Create(IAppHost host, TParameters parameters);
    }
}
