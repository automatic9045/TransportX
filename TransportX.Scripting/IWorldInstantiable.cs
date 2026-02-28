using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Worlds;

using TransportX.Scripting.Commands;

namespace TransportX.Scripting
{
    public interface IWorldInstantiable<T> where T : IWorldInstantiable<T>
    {
        static abstract T Create(ScriptWorld world);
    }
}
