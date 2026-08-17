using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Spatial;
using TransportX.Worlds;

namespace TransportX.Rendering.Pipelines
{
    public interface IRenderPass : IDisposable
    {
        void Execute(in RenderPassContext context, WorldBase world);
    }
}
