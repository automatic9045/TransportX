using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;
using TransportX.Rendering;
using TransportX.Worlds;

namespace TransportX.Scripting
{
    internal class ScriptModelCollection : ScriptDictionary<string, IModel>, IModelCollection
    {
        public ScriptModelCollection(IErrorCollector errorCollector) : base(errorCollector, "モデル", key => Model.Empty())
        {
        }

        public void Dispose()
        {
            foreach (IModel model in Values)
            {
                model.Dispose();
            }
        }
    }
}
