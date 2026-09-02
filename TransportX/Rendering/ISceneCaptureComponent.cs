using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;
using TransportX.Rendering.Pipelines;
using TransportX.Worlds;

namespace TransportX.Rendering
{
    public interface ISceneCaptureComponent : IComponent
    {
        void Initialize(in RenderResourceSet resources);
        void Capture(in RenderPassContext context, WorldBase world);
    }
}
