using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;

namespace TransportX.Scripting
{
    public interface IComponentCommand
    {
        IComponent Source { get; }
    }
}
