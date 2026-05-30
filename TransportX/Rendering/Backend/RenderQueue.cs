using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

using TransportX.Collections;

namespace TransportX.Rendering.Backend
{
    public class RenderQueue : IRenderQueue
    {
        private readonly ConcurrentDictionary<RenderLayer, ConcurrentDictionary<IModel, PooledBuffer<InstanceData>>> Instances = [];

        public RenderQueue()
        {
        }

        public void Clear()
        {
            foreach (ConcurrentDictionary<IModel, PooledBuffer<InstanceData>> perLayer in Instances.Values)
            {
                foreach (PooledBuffer<InstanceData> perModel in perLayer.Values)
                {
                    perModel.Clear();
                }
            }
        }

        public void Submit(RenderLayer layer, IModel model, in InstanceData instance)
        {
            ConcurrentDictionary<IModel, PooledBuffer<InstanceData>> perLayer = Instances.GetOrAdd(layer, layer => []);

            if (!perLayer.TryGetValue(model, out PooledBuffer<InstanceData>? perModel))
            {
                PooledBuffer<InstanceData> newBuffer = [];
                if (perLayer.TryAdd(model, newBuffer))
                {
                    perModel = newBuffer;
                }
                else
                {
                    newBuffer.Dispose();
                    perModel = perLayer[model];
                }
            }

            perModel.Add(instance);
        }

        public unsafe void Render(RenderLayer layer, in DrawContext context)
        {
            if (!Instances.TryGetValue(layer, out ConcurrentDictionary<IModel, PooledBuffer<InstanceData>>? perLayer)) return;

            foreach ((IModel model, PooledBuffer<InstanceData> perModel) in perLayer)
            {
                if (perModel.Count == 0) continue;

                ReadOnlySpan<InstanceData> fullSpan = perModel.Span;
                int maxInstanceCount = (int)context.InstanceBuffer.Description.ByteWidth / InstanceData.Size;

                for (int offset = 0; offset < fullSpan.Length; offset += maxInstanceCount)
                {
                    int count = int.Min(maxInstanceCount, fullSpan.Length - offset);
                    ReadOnlySpan<InstanceData> slice = fullSpan.Slice(offset, count);

                    MappedSubresource mapped = context.DeviceContext.Map(context.InstanceBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
                    Span<InstanceData> destination = new(mapped.DataPointer.ToPointer(), count);
                    slice.CopyTo(destination);
                    context.DeviceContext.Unmap(context.InstanceBuffer, 0);

                    DrawContext instancedContext = context with
                    {
                        InstanceCount = count,
                    };
                    model.Draw(instancedContext);
                }
            }
        }
    }
}
