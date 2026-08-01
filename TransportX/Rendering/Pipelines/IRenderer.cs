using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Cameras;
using TransportX.Worlds;

namespace TransportX.Rendering.Pipelines
{
    public interface IRenderer : IDisposable
    {
        void Render(ICamera camera, WorldBase world, TimeSpan elapsed);
    }
}
