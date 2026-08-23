using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Rendering.Pipelines
{
    public enum RenderPassOutputMode : uint
    {
        Deferred = 0,
        Forward = 1,
    }
}
