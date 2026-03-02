using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;
using TransportX.Diagnostics;

namespace TransportX.Scripting
{
    public interface ITemplateComponent<T> : IComponent
    {
        void Build(T parent, IErrorCollector errorCollector);
    }
}
