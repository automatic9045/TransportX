using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Dependency;

namespace TransportX
{
    public interface IApp : IDisposable
    {
        AppHost Host { get; }
    }
}
