using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;
using Vortice.Mathematics;

using TransportX.Rendering.Backend;
using TransportX.Spatial;

namespace TransportX.Rendering
{
    public readonly struct TransformedDrawContext
    {
        public required ID3D11DeviceContext DeviceContext { get; init; }
        public required IRenderQueue RenderQueue { get; init; }
        public required ChunkIndex ChunkOffset { get; init; }
        public required Matrix4x4 View { get; init; }
        public required Matrix4x4 Projection { get; init; }
        public required BoundingFrustum Frustum { get; init; }
        public RenderLayer Layer { get; init; } = RenderLayer.Normal;

        public TransformedDrawContext()
        {
        }

        public void DrawModel(IModel model, Matrix4x4 world)
        {
            InstanceData instanceData = new()
            {
                World = Matrix4x4.Transpose(world),
            };
            RenderQueue.Submit(model, instanceData);
        }
    }
}
