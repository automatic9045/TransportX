using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;
using Vortice.Mathematics;

using TransportX.Spatial;

namespace TransportX.Rendering
{
    public readonly struct LocatedDrawContext
    {
        public required ID3D11DeviceContext DeviceContext { get; init; }
        public required ID3D11Buffer SingleInstanceBuffer { get; init; }
        public required ID3D11Buffer MaterialBuffer { get; init; }
        public required PlateOffset PlateOffset { get; init; }
        public required Matrix4x4 View { get; init; }
        public required Matrix4x4 Projection { get; init; }
        public required BoundingFrustum Frustum { get; init; }
        public RenderPass Pass { get; init; } = RenderPass.Normal;

        public LocatedDrawContext()
        {
        }

        public unsafe void UpdateInstanceBuffer(ID3D11Buffer instanceBuffer, ReadOnlySpan<InstanceData> instances)
        {
            MappedSubresource mapped = DeviceContext.Map(instanceBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
            Span<InstanceData> destination = new(mapped.DataPointer.ToPointer(), instances.Length);
            instances.CopyTo(destination);
            DeviceContext.Unmap(instanceBuffer, 0);
        }

        public unsafe void UpdateSingleInstanceBuffer(InstanceData instance)
        {
            MappedSubresource mapped = DeviceContext.Map(SingleInstanceBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
            Unsafe.Write(mapped.DataPointer.ToPointer(), instance);
            DeviceContext.Unmap(SingleInstanceBuffer, 0);
        }
    }
}
