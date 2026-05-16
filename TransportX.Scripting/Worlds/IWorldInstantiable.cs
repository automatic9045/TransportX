using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Scripting.Worlds
{
    public interface IWorldInstantiable<T> where T : IWorldInstantiable<T>
    {
        static abstract T Create(ScriptWorld world);
    }
}
