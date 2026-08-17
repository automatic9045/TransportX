using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

namespace TransportX.Rendering.Pipelines
{
    public readonly struct RenderResourceSet : IDisposable
    {
        public required RenderContext Context { get; init; }

        public required ID3D11Buffer InstanceBuffer { get; init; }
        public required ID3D11Buffer MaterialBuffer { get; init; }
        public required ID3D11Buffer EnvironmentBuffer { get; init; }
        public required ID3D11Buffer SceneBuffer { get; init; }

        public RenderResourceSet()
        {
        }

        public void Dispose()
        {
            InstanceBuffer.Dispose();
            MaterialBuffer.Dispose();
            EnvironmentBuffer.Dispose();
            SceneBuffer.Dispose();
        }
    }
}
