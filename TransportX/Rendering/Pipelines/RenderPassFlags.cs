using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Rendering.Pipelines
{
    [Flags]
    public enum RenderPassFlags : uint
    {
        None = 0,
        Reflect = 1 << 0,
        DisableShadows = 1 << 1,
    }
}
