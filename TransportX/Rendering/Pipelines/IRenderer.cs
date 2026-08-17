using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

using TransportX.Cameras;
using TransportX.Worlds;

namespace TransportX.Rendering.Pipelines
{
    public interface IRenderer : IDisposable
    {
        private static readonly InputElementDescription[] DefaultInputElementsKey = [
            .. Vertex.InputElements,
            .. InstanceData.InputElements,
        ];
        public static ReadOnlySpan<InputElementDescription> DefaultInputElements => DefaultInputElementsKey;


        void Render(ICamera camera, WorldBase world, TimeSpan elapsed);
    }
}
