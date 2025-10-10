using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Rendering;

namespace Bus.Common.Worlds
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
