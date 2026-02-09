using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Rendering;

namespace TransportX.Worlds
{
    public class ModelCollection : Dictionary<string, IModel>, IModelCollection
    {
        public ModelCollection()
        {
        }
    }
}
