using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Worlds;

namespace TransportX.Scripting
{
    public interface IWorldComponentCommand
    {
        IWorldComponent Source { get; }
    }
}
