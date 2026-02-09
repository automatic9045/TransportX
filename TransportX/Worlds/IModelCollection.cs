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
        void IDisposable.Dispose()
        {
            foreach (IModel model in Values)
            {
                model.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }
}
