using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Rendering;

namespace TransportX.Worlds
{
    public interface IModelCollection : IDictionary<string, IModel>, IDisposable
    {
    }
}
